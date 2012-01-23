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
using NzbDrone.Core.Model.Sabnzbd;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class SabProviderTest : CoreTest
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
                .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=queue&output=json&start=0&limit=0&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                .Returns(File.ReadAllText(@".\Files\Queue.txt"));
        }

        private void WithEmptyQueue()
        {
            Mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=queue&output=json&start=0&limit=0&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                .Returns(File.ReadAllText(@".\Files\QueueEmpty.txt"));
        }



        private void WithFailResponse()
        {
            Mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadString(It.IsAny<String>())).Returns("failed");
        }

        [Test]
        public void add_url_should_format_request_properly()
        {
            Mocker.GetMock<HttpProvider>(MockBehavior.Strict)
                .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=addurl&name=http://www.nzbclub.com/nzb_download.aspx?mid=1950232&priority=0&pp=3&cat=tv&nzbname=This+is+an+Nzb&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                .Returns("ok");

            //Act
            bool result = Mocker.Resolve<SabProvider>().AddByUrl(
                "http://www.nzbclub.com/nzb_download.aspx?mid=1950232", "This is an Nzb");

            //Assert
            result.Should().BeTrue();
        }


        [Test]
        public void newzbing_add_url_should_format_request_properly()
        {
            Mocker.GetMock<HttpProvider>(MockBehavior.Strict)
                .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=addid&name=6107863&priority=0&pp=3&cat=tv&nzbname=This+is+an+Nzb&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                .Returns("ok");

            //Act
            bool result = Mocker.Resolve<SabProvider>().AddByUrl(
                "http://www.newzbin.com/browse/post/6107863/nzb", "This is an Nzb");

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void add_by_url_should_detect_and_handle_sab_errors()
        {
            WithFailResponse();

            //Act
            var sabProvider = Mocker.Resolve<SabProvider>();
            var result = sabProvider.AddByUrl("http://www.nzbclub.com/nzb_download.aspx?mid=1950232", "This is an nzb");

            //Assert
            Assert.IsFalse(result);
            ExceptionVerification.ExpectedWarns(1);
        }


        [TestCase(1, new[] { 2 }, "My Episode Title", QualityTypes.DVD, false, "My Series Name - 1x2 - My Episode Title [DVD]")]
        [TestCase(1, new[] { 2 }, "My Episode Title", QualityTypes.DVD, true, "My Series Name - 1x2 - My Episode Title [DVD] [Proper]")]
        [TestCase(1, new[] { 2 }, "", QualityTypes.DVD, true, "My Series Name - 1x2 -  [DVD] [Proper]")]
        [TestCase(1, new[] { 2, 4 }, "My Episode Title", QualityTypes.HDTV, false, "My Series Name - 1x2-1x4 - My Episode Title [HDTV]")]
        [TestCase(1, new[] { 2, 4 }, "My Episode Title", QualityTypes.HDTV, true, "My Series Name - 1x2-1x4 - My Episode Title [HDTV] [Proper]")]
        [TestCase(1, new[] { 2, 4 }, "", QualityTypes.HDTV, true, "My Series Name - 1x2-1x4 -  [HDTV] [Proper]")]
        public void create_proper_sab_titles(int seasons, int[] episodes, string title, QualityTypes quality, bool proper, string expected)
        {
            var series = Builder<Series>.CreateNew()
                .With(c => c.Title = "My Series Name")
                .Build();

            var parsResult = new EpisodeParseResult()
            {
                AirDate = DateTime.Now,
                EpisodeNumbers = episodes.ToList(),
                Quality = new Quality(quality, proper),
                SeasonNumber = seasons,
                Series = series,
                EpisodeTitle = title
            };

            //Act
            var actual = Mocker.Resolve<SabProvider>().GetSabTitle(parsResult);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestCase(true, "My Series Name - Season 1 [Bluray720p] [Proper]")]
        [TestCase(false, "My Series Name - Season 1 [Bluray720p]")]
        public void create_proper_sab_season_title(bool proper, string expected)
        {


            var series = Builder<Series>.CreateNew()
                .With(c => c.Title = "My Series Name")
                .Build();

            var parsResult = new EpisodeParseResult()
            {
                AirDate = DateTime.Now,
                Quality = new Quality(QualityTypes.Bluray720p, proper),
                SeasonNumber = 1,
                Series = series,
                EpisodeTitle = "My Episode Title",
                FullSeason = true
            };

            //Act
            var actual = Mocker.Resolve<SabProvider>().GetSabTitle(parsResult);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestCase(true, "My Series Name - 2011-12-01 - My Episode Title [Bluray720p] [Proper]")]
        [TestCase(false, "My Series Name - 2011-12-01 - My Episode Title [Bluray720p]")]
        public void create_proper_sab_daily_titles(bool proper, string expected)
        {
            var series = Builder<Series>.CreateNew()
                .With(c => c.IsDaily = true)
                .With(c => c.Title = "My Series Name")
                .Build();

            var parsResult = new EpisodeParseResult
            {
                AirDate = new DateTime(2011, 12, 1),
                Quality = new Quality(QualityTypes.Bluray720p, proper),
                Series = series,
                EpisodeTitle = "My Episode Title",
            };

            //Act
            var actual = Mocker.Resolve<SabProvider>().GetSabTitle(parsResult);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void should_be_able_to_get_categories_when_config_is_passed_in()
        {
            //Setup
            const string host = "192.168.5.55";
            const int port = 2222;
            const string apikey = "5c770e3197e4fe763423ee7c392c25d1";
            const string username = "admin";
            const string password = "pass";



            Mocker.GetMock<HttpProvider>(MockBehavior.Strict)
                .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=get_cats&output=json&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                .Returns(File.ReadAllText(@".\Files\Categories_json.txt"));

            //Act
            var result = Mocker.Resolve<SabProvider>().GetCategories(host, port, apikey, username, password);

            //Assert
            result.Should().NotBeNull();
            result.categories.Should().HaveCount(c => c > 0);
        }

        [Test]
        public void should_be_able_to_get_categories_using_config()
        {
            Mocker.GetMock<HttpProvider>(MockBehavior.Strict)
                 .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=get_cats&output=json&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                 .Returns(File.ReadAllText(@".\Files\Categories_json.txt"));

            //Act
            var result = Mocker.Resolve<SabProvider>().GetCategories();

            //Assert
            result.Should().NotBeNull();
            result.categories.Should().HaveCount(c => c > 0);
        }

        [Test]
        public void GetQueue_should_return_an_empty_list_when_the_queue_is_empty()
        {
            Mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=queue&output=json&start=0&limit=0&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                .Returns(File.ReadAllText(@".\Files\QueueEmpty.txt"));

            //Act
            var result = Mocker.Resolve<SabProvider>().GetQueue();

            //Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void GetQueue_should_return_an_empty_list_when_there_is_an_error_getting_the_queue()
        {
            Mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=queue&output=json&start=0&limit=0&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                .Returns(File.ReadAllText(@".\Files\JsonError.txt"));

            //Act
            Assert.Throws<ApplicationException>(() => Mocker.Resolve<SabProvider>().GetQueue(), "API Key Incorrect");
        }

        [Test]
        public void GetQueue_should_return_a_list_with_items_when_the_queue_has_items()
        {
            WithFullQueue();
            //Act
            var result = Mocker.Resolve<SabProvider>().GetQueue();

            //Assert
            result.Should().HaveCount(3);
        }

        [Test]
        public void GetHistory_should_return_a_list_with_items_when_the_history_has_items()
        {
            Mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=history&output=json&start=0&limit=0&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                .Returns(File.ReadAllText(@".\Files\History.txt"));

            //Act
            var result = Mocker.Resolve<SabProvider>().GetHistory();

            //Assert
            result.Should().HaveCount(1);
        }

        [Test]
        public void GetHistory_should_return_an_empty_list_when_the_queue_is_empty()
        {
            Mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=history&output=json&start=0&limit=0&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                .Returns(File.ReadAllText(@".\Files\HistoryEmpty.txt"));

            //Act
            var result = Mocker.Resolve<SabProvider>().GetHistory();

            //Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void GetHistory_should_return_an_empty_list_when_there_is_an_error_getting_the_queue()
        {
            Mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=history&output=json&start=0&limit=0&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                .Returns(File.ReadAllText(@".\Files\JsonError.txt"));

            //Act
            Assert.Throws<ApplicationException>(() => Mocker.Resolve<SabProvider>().GetHistory(), "API Key Incorrect");
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
                                  Series = new Series { Title = "30 Rock" },
                              };


            var result = Mocker.Resolve<SabProvider>().IsInQueue(parseResult);

            result.Should().BeTrue();
        }

        [TestCase(2, new[] { 5 }, "30 Rock", QualityTypes.Bluray1080p, true, Description = "Same Series, Diffrent Season, Episode")]
        [TestCase(1, new[] { 6 }, "30 Rock", QualityTypes.Bluray1080p, true, Description = "Same series, diffrent episodes")]
        [TestCase(1, new[] { 6,7,8 }, "30 Rock", QualityTypes.Bluray1080p, true, Description = "Same series, diffrent episodes")]
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
        public void IsInQueue_should_find_same_or_lower_quality_episode_queue(int season, int[] episodes, string title, QualityTypes qualityType, bool proper)
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

            result.Should().BeTrue();
        }

        [Test]
        public void IsInQueue_should_return_false_if_queue_is_empty()
        {
            WithEmptyQueue();

            var parseResult = new EpisodeParseResult
            {
                EpisodeTitle = "Title",
                EpisodeNumbers = new List<int>{1},
                SeasonNumber = 2,
                Quality = new Quality { QualityType  = QualityTypes.Bluray1080p, Proper = true },
                Series = new Series { Title = "Test" },
            };

            var result = Mocker.Resolve<SabProvider>().IsInQueue(parseResult);

            result.Should().BeFalse();

        }


    }
}