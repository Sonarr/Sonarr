// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model.Xbmc;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Xbmc;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class XbmcProviderTest : TestBase
    {
        [Test]
        public void JsonEror_true()
        {
            //Setup
            var mocker = new AutoMoqer();
            var response = "{\"error\":{\"code\":-32601,\"message\":\"Method not found.\"},\"id\":10,\"jsonrpc\":\"2.0\"}";

            //Act
            var result = mocker.Resolve<XbmcProvider>().CheckForJsonError(response);

            //Assert
            Assert.AreEqual(true, result);
        }

        [Test]
        public void JsonEror_false()
        {
            //Setup
            var mocker = new AutoMoqer();
            var reposnse = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"version\":3}}";

            //Act
            var result = mocker.Resolve<XbmcProvider>().CheckForJsonError(reposnse);

            //Assert
            Assert.AreEqual(false, result);
        }

        [TestCase(3)]
        [TestCase(2)]
        [TestCase(0)]
        public void GetJsonVersion(int number)
        {
            //Setup
            var mocker = new AutoMoqer();

            var message = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"version\":" + number + "}}";

            var fakeHttp = mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.PostCommand("localhost:8080", "xbmc", "xbmc", It.IsAny<string>()))
                .Returns(message);

            //Act
            var result = mocker.Resolve<XbmcProvider>().GetJsonVersion("localhost:8080", "xbmc", "xbmc");

            //Assert
            Assert.AreEqual(number, result);
        }

        [Test]
        public void GetJsonVersion_error()
        {
            //Setup
            var mocker = new AutoMoqer();

            var message = "{\"error\":{\"code\":-32601,\"message\":\"Method not found.\"},\"id\":10,\"jsonrpc\":\"2.0\"}";

            var fakeHttp = mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.PostCommand("localhost:8080", "xbmc", "xbmc", It.IsAny<string>()))
                .Returns(message);

            //Act
            var result = mocker.Resolve<XbmcProvider>().GetJsonVersion("localhost:8080", "xbmc", "xbmc");

            //Assert
            Assert.AreEqual(0, result);
        }

        [TestCase(false, false, false)]
        [TestCase(true, true, true)]
        [TestCase(true, false, false)]
        [TestCase(true, true, false)]
        [TestCase(false, true, false)]
        [TestCase(false, true, true)]
        [TestCase(false, false, true)]
        [TestCase(true, false, true)]
        public void GetActivePlayers(bool audio, bool picture, bool video)
        {
            //Setup
            var mocker = new AutoMoqer();

            var message = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"audio\":"
                + audio.ToString().ToLower()
                + ",\"picture\":"
                + picture.ToString().ToLower()
                + ",\"video\":"
                + video.ToString().ToLower()
                + "}}";

            var fakeHttp = mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.PostCommand("localhost:8080", "xbmc", "xbmc", It.IsAny<string>()))
                .Returns(message);

            //Act
            var result = mocker.Resolve<XbmcProvider>().GetActivePlayers("localhost:8080", "xbmc", "xbmc");

            //Assert
            Assert.AreEqual(audio, result["audio"]);
            Assert.AreEqual(picture, result["picture"]);
            Assert.AreEqual(video, result["video"]);
        }

        [Test]
        public void GetTvShowsJson()
        {
            //Setup
            var mocker = new AutoMoqer();

            var message = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"limits\":{\"end\":5,\"start\":0,\"total\":5},\"tvshows\":[{\"file\":\"smb://HOMESERVER/TV/7th Heaven/\",\"imdbnumber\":\"73928\",\"label\":\"7th Heaven\",\"tvshowid\":3},{\"file\":\"smb://HOMESERVER/TV/8 Simple Rules/\",\"imdbnumber\":\"78461\",\"label\":\"8 Simple Rules\",\"tvshowid\":4},{\"file\":\"smb://HOMESERVER/TV/24-7 Penguins-Capitals- Road to the NHL Winter Classic/\",\"imdbnumber\":\"213041\",\"label\":\"24/7 Penguins/Capitals: Road to the NHL Winter Classic\",\"tvshowid\":1},{\"file\":\"smb://HOMESERVER/TV/30 Rock/\",\"imdbnumber\":\"79488\",\"label\":\"30 Rock\",\"tvshowid\":2},{\"file\":\"smb://HOMESERVER/TV/90210/\",\"imdbnumber\":\"82716\",\"label\":\"90210\",\"tvshowid\":5}]}}";

            var fakeHttp = mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.PostCommand("localhost:8080", "xbmc", "xbmc", It.IsAny<string>()))
                .Returns(message);

            //Act
            var result = mocker.Resolve<XbmcProvider>().GetTvShowsJson("localhost:8080", "xbmc", "xbmc");

            //Assert
            Assert.AreEqual(5, result.Count);
            result.Should().Contain(s => s.ImdbNumber == 79488);
        }

        [Test]
        public void Notify_true()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);

            var header = "NzbDrone Test";
            var message = "Test Message!";

            var fakeConfig = mocker.GetMock<ConfigProvider>();
            fakeConfig.SetupGet(s => s.XbmcHosts).Returns("localhost:8080");

            //var fakeUdpProvider = mocker.GetMock<EventClient>();
            var fakeEventClient = mocker.GetMock<EventClientProvider>();
            fakeEventClient.Setup(s => s.SendNotification(header, message, IconType.Jpeg, "NzbDrone.jpg", "localhost")).Returns(true);

            //Act
            mocker.Resolve<XbmcProvider>().Notify(header, message);

            //Assert
            mocker.VerifyAllMocks();
        }

        [Test]
        public void SendCommand()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);

            var host = "localhost:8080";
            var command = "ExecBuiltIn(CleanLibrary(video))";
            var username = "xbmc";
            var password = "xbmc";

            var url = String.Format("http://localhost:8080/xbmcCmds/xbmcHttp?command=ExecBuiltIn(CleanLibrary(video))");

            //var fakeUdpProvider = mocker.GetMock<EventClient>();
            var fakeHttp = mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.DownloadString(url, username, password)).Returns("Ok\n");

            //Act
            var result = mocker.Resolve<XbmcProvider>().SendCommand(host, command, username, username);

            //Assert
            mocker.VerifyAllMocks();
            Assert.AreEqual("Ok\n", result);
        }

        [Test]
        public void GetXbmcSeriesPath_true()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);

            var queryResult = @"<xml><record><field>smb://xbmc:xbmc@HOMESERVER/TV/30 Rock/</field></record></xml>";

            var host = "localhost:8080";
            var username = "xbmc";
            var password = "xbmc";

            var setResponseUrl = "http://localhost:8080/xbmcCmds/xbmcHttp?command=SetResponseFormat(webheader;false;webfooter;false;header;<xml>;footer;</xml>;opentag;<tag>;closetag;</tag>;closefinaltag;false)";
            var resetResponseUrl = "http://localhost:8080/xbmcCmds/xbmcHttp?command=SetResponseFormat()";
            var query = String.Format("http://localhost:8080/xbmcCmds/xbmcHttp?command=QueryVideoDatabase(select path.strPath from path, tvshow, tvshowlinkpath where tvshow.c12 = 79488 and tvshowlinkpath.idShow = tvshow.idShow and tvshowlinkpath.idPath = path.idPath)");


            //var fakeUdpProvider = mocker.GetMock<EventClient>();
            var fakeHttp = mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.DownloadString(setResponseUrl, username, password)).Returns("<xml><tag>OK</xml>");
            fakeHttp.Setup(s => s.DownloadString(resetResponseUrl, username, password)).Returns(@"<html>
                                                                                                    <li>OK
                                                                                                    </html>");
            fakeHttp.Setup(s => s.DownloadString(query, username, password)).Returns(queryResult);

            //Act
            var result = mocker.Resolve<XbmcProvider>().GetXbmcSeriesPath(host, 79488, username, username);

            //Assert
            mocker.VerifyAllMocks();
            Assert.AreEqual("smb://xbmc:xbmc@HOMESERVER/TV/30 Rock/", result);
        }

        [Test]
        public void GetXbmcSeriesPath_false()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);

            var queryResult = @"<xml></xml>";

            var host = "localhost:8080";
            var username = "xbmc";
            var password = "xbmc";

            var setResponseUrl = "http://localhost:8080/xbmcCmds/xbmcHttp?command=SetResponseFormat(webheader;false;webfooter;false;header;<xml>;footer;</xml>;opentag;<tag>;closetag;</tag>;closefinaltag;false)";
            var resetResponseUrl = "http://localhost:8080/xbmcCmds/xbmcHttp?command=SetResponseFormat()";
            var query = String.Format("http://localhost:8080/xbmcCmds/xbmcHttp?command=QueryVideoDatabase(select path.strPath from path, tvshow, tvshowlinkpath where tvshow.c12 = 79488 and tvshowlinkpath.idShow = tvshow.idShow and tvshowlinkpath.idPath = path.idPath)");


            //var fakeUdpProvider = mocker.GetMock<EventClient>();
            var fakeHttp = mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.DownloadString(setResponseUrl, username, password)).Returns("<xml><tag>OK</xml>");
            fakeHttp.Setup(s => s.DownloadString(resetResponseUrl, username, password)).Returns(@"<html>
                                                                                                    <li>OK
                                                                                                    </html>");
            fakeHttp.Setup(s => s.DownloadString(query, username, password)).Returns(queryResult);

            //Act
            var result = mocker.Resolve<XbmcProvider>().GetXbmcSeriesPath(host, 79488, username, username);

            //Assert
            mocker.VerifyAllMocks();
            Assert.AreEqual("", result);
        }

        [Test]
        public void Clean()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);

            var fakeConfig = mocker.GetMock<ConfigProvider>();
            fakeConfig.SetupGet(s => s.XbmcHosts).Returns("localhost:8080");

            var fakeEventClient = mocker.GetMock<EventClientProvider>();
            fakeEventClient.Setup(s => s.SendAction("localhost", ActionType.ExecBuiltin, "ExecBuiltIn(CleanLibrary(video))")).Returns(true);

            //Act
            mocker.Resolve<XbmcProvider>().Clean();

            //Assert
            mocker.VerifyAllMocks();
        }

        [Test]
        public void UpdateWithHttp_Single()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Default);

            var host = "localhost:8080";
            var username = "xbmc";
            var password = "xbmc";
            var queryResult = @"<xml><record><field>smb://xbmc:xbmc@HOMESERVER/TV/30 Rock/</field></record></xml>";
            var queryUrl = "http://localhost:8080/xbmcCmds/xbmcHttp?command=QueryVideoDatabase(select path.strPath from path, tvshow, tvshowlinkpath where tvshow.c12 = 79488 and tvshowlinkpath.idShow = tvshow.idShow and tvshowlinkpath.idPath = path.idPath)";
            var url = "http://localhost:8080/xbmcCmds/xbmcHttp?command=ExecBuiltIn(UpdateLibrary(video,smb://xbmc:xbmc@HOMESERVER/TV/30 Rock/))";

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 79488)
                .With(s => s.Title = "30 Rock")
                .Build();

            var fakeHttp = mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.DownloadString(queryUrl, username, password)).Returns(queryResult);
            fakeHttp.Setup(s => s.DownloadString(url, username, password));

            //Act
            mocker.Resolve<XbmcProvider>().UpdateWithHttp(fakeSeries, host, username, password);

            //Assert
            mocker.VerifyAllMocks();
        }

        [Test]
        public void UpdateWithHttp_All()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Default);

            var host = "localhost:8080";
            var username = "xbmc";
            var password = "xbmc";
            var queryResult = @"<xml></xml>";
            var queryUrl = "http://localhost:8080/xbmcCmds/xbmcHttp?command=QueryVideoDatabase(select path.strPath from path, tvshow, tvshowlinkpath where tvshow.c12 = 79488 and tvshowlinkpath.idShow = tvshow.idShow and tvshowlinkpath.idPath = path.idPath)";
            var url = "http://localhost:8080/xbmcCmds/xbmcHttp?command=ExecBuiltIn(UpdateLibrary(video))";

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 79488)
                .With(s => s.Title = "30 Rock")
                .Build();

            var fakeHttp = mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.DownloadString(queryUrl, username, password)).Returns(queryResult);
            fakeHttp.Setup(s => s.DownloadString(url, username, password));

            //Act
            mocker.Resolve<XbmcProvider>().UpdateWithHttp(fakeSeries, host, username, password);

            //Assert
            mocker.VerifyAllMocks();
        }

        [Test]
        public void UpdateWithJson_Single()
        {
            //Setup
            var mocker = new AutoMoqer();

            var host = "localhost:8080";
            var username = "xbmc";
            var password = "xbmc";
            var serializedQuery = "{\"jsonrpc\":\"2.0\",\"method\":\"VideoLibrary.GetTvShows\",\"params\":{\"fields\":[\"file\",\"imdbnumber\"]},\"id\":10}";
            var tvshows = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"limits\":{\"end\":5,\"start\":0,\"total\":5},\"tvshows\":[{\"file\":\"smb://HOMESERVER/TV/7th Heaven/\",\"imdbnumber\":\"73928\",\"label\":\"7th Heaven\",\"tvshowid\":3},{\"file\":\"smb://HOMESERVER/TV/8 Simple Rules/\",\"imdbnumber\":\"78461\",\"label\":\"8 Simple Rules\",\"tvshowid\":4},{\"file\":\"smb://HOMESERVER/TV/24-7 Penguins-Capitals- Road to the NHL Winter Classic/\",\"imdbnumber\":\"213041\",\"label\":\"24/7 Penguins/Capitals: Road to the NHL Winter Classic\",\"tvshowid\":1},{\"file\":\"smb://HOMESERVER/TV/30 Rock/\",\"imdbnumber\":\"79488\",\"label\":\"30 Rock\",\"tvshowid\":2},{\"file\":\"smb://HOMESERVER/TV/90210/\",\"imdbnumber\":\"82716\",\"label\":\"90210\",\"tvshowid\":5}]}}";

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 79488)
                .With(s => s.Title = "30 Rock")
                .Build();

            var fakeHttp = mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.PostCommand(host, username, password, serializedQuery))
                .Returns(tvshows);

            var fakeEventClient = mocker.GetMock<EventClientProvider>();
            fakeEventClient.Setup(s => s.SendAction("localhost", ActionType.ExecBuiltin, "ExecBuiltIn(UpdateLibrary(video,smb://HOMESERVER/TV/30 Rock/))"));

            //Act
            mocker.Resolve<XbmcProvider>().UpdateWithJson(fakeSeries, host, username, password);

            //Assert
            mocker.VerifyAllMocks();
        }

        [Test]
        public void UpdateWithJson_All()
        {
            //Setup
            var mocker = new AutoMoqer();

            var host = "localhost:8080";
            var username = "xbmc";
            var password = "xbmc";
            var serializedQuery = "{\"jsonrpc\":\"2.0\",\"method\":\"VideoLibrary.GetTvShows\",\"params\":{\"fields\":[\"file\",\"imdbnumber\"]},\"id\":10}";
            var tvshows = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"limits\":{\"end\":5,\"start\":0,\"total\":5},\"tvshows\":[{\"file\":\"smb://HOMESERVER/TV/7th Heaven/\",\"imdbnumber\":\"73928\",\"label\":\"7th Heaven\",\"tvshowid\":3},{\"file\":\"smb://HOMESERVER/TV/8 Simple Rules/\",\"imdbnumber\":\"78461\",\"label\":\"8 Simple Rules\",\"tvshowid\":4},{\"file\":\"smb://HOMESERVER/TV/24-7 Penguins-Capitals- Road to the NHL Winter Classic/\",\"imdbnumber\":\"213041\",\"label\":\"24/7 Penguins/Capitals: Road to the NHL Winter Classic\",\"tvshowid\":1},{\"file\":\"smb://HOMESERVER/TV/90210/\",\"imdbnumber\":\"82716\",\"label\":\"90210\",\"tvshowid\":5}]}}";

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 79488)
                .With(s => s.Title = "30 Rock")
                .Build();

            var fakeHttp = mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.PostCommand(host, username, password, serializedQuery))
                .Returns(tvshows);

            var fakeEventClient = mocker.GetMock<EventClientProvider>();
            fakeEventClient.Setup(s => s.SendAction("localhost", ActionType.ExecBuiltin, "ExecBuiltIn(UpdateLibrary(video))"));

            //Act
            mocker.Resolve<XbmcProvider>().UpdateWithJson(fakeSeries, host, username, password);

            //Assert
            mocker.VerifyAllMocks();
        }
    }
}