using System;
using System.IO;
using System.Linq;
using AutoMoq;
using MbUnit.Framework;
using Moq;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class SabControllerTest
    {
        [Test]
        public void AddByUrlSuccess()
        {
            //Setup
            string sabHost = "192.168.5.55";
            string sabPort = "2222";
            string apikey = "5c770e3197e4fe763423ee7c392c25d1";
            string username = "admin";
            string password = "pass";
            string priority = "0";
            string category = "tv";


            var mocker = new AutoMoqer();

            var fakeConfig = mocker.GetMock<ConfigProvider>();
            fakeConfig.Setup(c => c.GetValue("SabHost", String.Empty, false))
                .Returns(sabHost);
            fakeConfig.Setup(c => c.GetValue("SabPort", String.Empty, false))
                .Returns(sabPort);
            fakeConfig.Setup(c => c.GetValue("SabApiKey", String.Empty, false))
                .Returns(apikey);
            fakeConfig.Setup(c => c.GetValue("SabUsername", String.Empty, false))
                .Returns(username);
            fakeConfig.Setup(c => c.GetValue("SabPassword", String.Empty, false))
                .Returns(password);
            fakeConfig.Setup(c => c.GetValue("SabTvPriority", String.Empty, false))
                .Returns(priority);
            fakeConfig.Setup(c => c.GetValue("SabTvCategory", String.Empty, true))
                .Returns(category);

            mocker.GetMock<HttpProvider>()
                .Setup(
                    s =>
                    s.DownloadString(
                        "http://192.168.5.55:2222/api?mode=addurl&name=http://www.nzbclub.com/nzb_download.aspx?mid=1950232&priority=0&cat=tv&nzbname=This+is+an+Nzb&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                .Returns("ok");

            //Act
            bool result = mocker.Resolve<SabProvider>().AddByUrl(
                "http://www.nzbclub.com/nzb_download.aspx?mid=1950232", "This is an Nzb");

            //Assert
            Assert.AreEqual(true, result);
        }

        [Test]
        public void AddByUrlError()
        {
            //Setup
            string sabHost = "192.168.5.55";
            string sabPort = "2222";
            string apikey = "5c770e3197e4fe763423ee7c392c25d1";
            string username = "admin";
            string password = "pass";
            string priority = "0";
            string category = "tv";

            var mocker = new AutoMoqer();

            var fakeConfig = mocker.GetMock<ConfigProvider>();
            fakeConfig.Setup(c => c.GetValue("SabHost", String.Empty, false)).Returns(sabHost);
            fakeConfig.Setup(c => c.GetValue("SabPort", String.Empty, false)).Returns(sabPort);
            fakeConfig.Setup(c => c.GetValue("SabApiKey", String.Empty, false)).Returns(apikey);
            fakeConfig.Setup(c => c.GetValue("SabUsername", String.Empty, false)).Returns(username);
            fakeConfig.Setup(c => c.GetValue("SabPassword", String.Empty, false)).Returns(password);
            fakeConfig.Setup(c => c.GetValue("SabTvPriority", String.Empty, false)).Returns(priority);
            fakeConfig.Setup(c => c.GetValue("SabTvCategory", String.Empty, true)).Returns(category);

            mocker.GetMock<HttpProvider>()
                .Setup(
                    s =>
                    s.DownloadString(
                        "http://192.168.5.55:2222/api?mode=addurl&name=http://www.nzbclub.com/nzb_download.aspx?mid=1950232&priority=0&cat=tv&nzbname=This+is+an+Nzb&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                .Returns("error");

            //Act
            bool result = mocker.Resolve<SabProvider>().AddByUrl(
                "http://www.nzbclub.com/nzb_download.aspx?mid=1950232", "This is an Nzb");

            //Assert
            Assert.AreEqual(false, result);
        }

        [Test]
        public void IsInQueue_True()
        {
            //Setup
            string sabHost = "192.168.5.55";
            string sabPort = "2222";
            string apikey = "5c770e3197e4fe763423ee7c392c25d1";
            string username = "admin";
            string password = "pass";

            var mocker = new AutoMoqer();

            var fakeConfig = mocker.GetMock<ConfigProvider>();
            fakeConfig.Setup(c => c.GetValue("SabHost", String.Empty, false)).Returns(sabHost);
            fakeConfig.Setup(c => c.GetValue("SabPort", String.Empty, false)).Returns(sabPort);
            fakeConfig.Setup(c => c.GetValue("SabApiKey", String.Empty, false)).Returns(apikey);
            fakeConfig.Setup(c => c.GetValue("SabUsername", String.Empty, false)).Returns(username);
            fakeConfig.Setup(c => c.GetValue("SabPassword", String.Empty, false)).Returns(password);

            mocker.GetMock<HttpProvider>()
                .Setup(
                    s =>
                    s.DownloadString(
                        "http://192.168.5.55:2222/api?mode=queue&output=xml&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                .Returns(new StreamReader(@".\Files\Queue.xml").ReadToEnd());

            //Act
            bool result = mocker.Resolve<SabProvider>().IsInQueue("Ubuntu Test");

            //Assert
            Assert.AreEqual(true, result);
        }

        [Test]
        public void IsInQueue_False_Empty()
        {
            //Setup
            string sabHost = "192.168.5.55";
            string sabPort = "2222";
            string apikey = "5c770e3197e4fe763423ee7c392c25d1";
            string username = "admin";
            string password = "pass";

            var mocker = new AutoMoqer();

            var fakeConfig = mocker.GetMock<ConfigProvider>();
            fakeConfig.Setup(c => c.GetValue("SabHost", String.Empty, false)).Returns(sabHost);
            fakeConfig.Setup(c => c.GetValue("SabPort", String.Empty, false)).Returns(sabPort);
            fakeConfig.Setup(c => c.GetValue("SabApiKey", String.Empty, false)).Returns(apikey);
            fakeConfig.Setup(c => c.GetValue("SabUsername", String.Empty, false)).Returns(username);
            fakeConfig.Setup(c => c.GetValue("SabPassword", String.Empty, false)).Returns(password);

            mocker.GetMock<HttpProvider>()
                .Setup(
                    s =>
                    s.DownloadString(
                        "http://192.168.5.55:2222/api?mode=queue&output=xml&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                .Returns(new StreamReader(@".\Files\QueueEmpty.xml").ReadToEnd());

            //Act
            bool result = mocker.Resolve<SabProvider>().IsInQueue(String.Empty);

            //Assert
            Assert.AreEqual(false, result);
        }

        [Test]
        public void IsInQueue_False_Error()
        {
            //Setup
            string sabHost = "192.168.5.55";
            string sabPort = "2222";
            string apikey = "5c770e3197e4fe763423ee7c392c25d1";
            string username = "admin";
            string password = "pass";

            var mocker = new AutoMoqer();

            var fakeConfig = mocker.GetMock<ConfigProvider>();
            fakeConfig.Setup(c => c.GetValue("SabHost", String.Empty, false)).Returns(sabHost);
            fakeConfig.Setup(c => c.GetValue("SabPort", String.Empty, false)).Returns(sabPort);
            fakeConfig.Setup(c => c.GetValue("SabApiKey", String.Empty, false)).Returns(apikey);
            fakeConfig.Setup(c => c.GetValue("SabUsername", String.Empty, false)).Returns(username);
            fakeConfig.Setup(c => c.GetValue("SabPassword", String.Empty, false)).Returns(password);

            mocker.GetMock<HttpProvider>()
                .Setup(
                    s =>
                    s.DownloadString(
                        "http://192.168.5.55:2222/api?mode=queue&output=xml&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                .Returns(new StreamReader(@".\Files\QueueError.xml").ReadToEnd());


            //Act
            bool result = mocker.Resolve<SabProvider>().IsInQueue(String.Empty);

            //Assert
            Assert.AreEqual(false, result);
        }

        [Test]
        [Row(1, new[] { 2 }, "My Episode Title", QualityTypes.DVD, false, "My Series Name - 1x2 - My Episode Title [DVD]")]
        [Row(1, new[] { 2 }, "My Episode Title", QualityTypes.DVD, true, "My Series Name - 1x2 - My Episode Title [DVD] [Proper]")]
        [Row(1, new[] { 2 }, "", QualityTypes.DVD, true, "My Series Name - 1x2 -  [DVD] [Proper]")]
        [Row(1, new[] { 2, 4 }, "My Episode Title", QualityTypes.HDTV, false, "My Series Name - 1x2-1x4 - My Episode Title [HDTV]")]
        [Row(1, new[] { 2, 4 }, "My Episode Title", QualityTypes.HDTV, true, "My Series Name - 1x2-1x4 - My Episode Title [HDTV] [Proper]")]
        [Row(1, new[] { 2, 4 }, "", QualityTypes.HDTV, true, "My Series Name - 1x2-1x4 -  [HDTV] [Proper]")]
        public void sab_title(int seasons, int[] episodes, string title, QualityTypes quality, bool proper, string excpected)
        {
            var mocker = new AutoMoqer();

            var parsResult = new EpisodeParseResult()
            {
                SeriesId = 12,
                AirDate = DateTime.Now,
                Episodes = episodes.ToList(),
                Proper = proper,
                Quality = quality,
                SeasonNumber = seasons,
                EpisodeTitle = title,
                FolderName = "My Series Name"
            };

            //Act
            var actual = mocker.Resolve<SabProvider>().GetSabTitle(parsResult);

            //Assert
            Assert.AreEqual(excpected, actual);
        }
    }
}