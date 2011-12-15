// ReSharper disable RedundantUsingDirective

using System;

using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model.Xbmc;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Xbmc;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class XbmcProviderTest : CoreTest
    {
        [Test]
        public void JsonError_true()
        {
            //Setup
            
            var response = "{\"error\":{\"code\":-32601,\"message\":\"Method not found.\"},\"id\":10,\"jsonrpc\":\"2.0\"}";

            //Act
            var result = Mocker.Resolve<XbmcProvider>().CheckForJsonError(response);

            //Assert
            Assert.AreEqual(true, result);
        }

        [Test]
        public void JsonError_true_empty_response()
        {
            //Setup
            
            var response = String.Empty;

            //Act
            var result = Mocker.Resolve<XbmcProvider>().CheckForJsonError(response);

            //Assert
            Assert.AreEqual(true, result);
        }

        [Test]
        public void JsonError_false()
        {
            //Setup
            
            var reposnse = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"version\":3}}";

            //Act
            var result = Mocker.Resolve<XbmcProvider>().CheckForJsonError(reposnse);

            //Assert
            Assert.AreEqual(false, result);
        }

        [TestCase(3)]
        [TestCase(2)]
        [TestCase(0)]
        public void GetJsonVersion(int number)
        {
            //Setup
            

            var message = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"version\":" + number + "}}";

            var fakeHttp = Mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.PostCommand("localhost:8080", "xbmc", "xbmc", It.IsAny<string>()))
                .Returns(message);

            //Act
            var result = Mocker.Resolve<XbmcProvider>().GetJsonVersion("localhost:8080", "xbmc", "xbmc");

            //Assert
            Assert.AreEqual(number, result);
        }

        [Test]
        public void GetJsonVersion_error()
        {
            //Setup
            

            var message = "{\"error\":{\"code\":-32601,\"message\":\"Method not found.\"},\"id\":10,\"jsonrpc\":\"2.0\"}";

            var fakeHttp = Mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.PostCommand("localhost:8080", "xbmc", "xbmc", It.IsAny<string>()))
                .Returns(message);

            //Act
            var result = Mocker.Resolve<XbmcProvider>().GetJsonVersion("localhost:8080", "xbmc", "xbmc");

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
            

            var message = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"audio\":"
                + audio.ToString().ToLower()
                + ",\"picture\":"
                + picture.ToString().ToLower()
                + ",\"video\":"
                + video.ToString().ToLower()
                + "}}";

            var fakeHttp = Mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.PostCommand("localhost:8080", "xbmc", "xbmc", It.IsAny<string>()))
                .Returns(message);

            //Act
            var result = Mocker.Resolve<XbmcProvider>().GetActivePlayers("localhost:8080", "xbmc", "xbmc");

            //Assert
            Assert.AreEqual(audio, result["audio"]);
            Assert.AreEqual(picture, result["picture"]);
            Assert.AreEqual(video, result["video"]);
        }

        [Test]
        public void GetTvShowsJson()
        {
            //Setup
            

            var message = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"limits\":{\"end\":5,\"start\":0,\"total\":5},\"tvshows\":[{\"file\":\"smb://HOMESERVER/TV/7th Heaven/\",\"imdbnumber\":\"73928\",\"label\":\"7th Heaven\",\"tvshowid\":3},{\"file\":\"smb://HOMESERVER/TV/8 Simple Rules/\",\"imdbnumber\":\"78461\",\"label\":\"8 Simple Rules\",\"tvshowid\":4},{\"file\":\"smb://HOMESERVER/TV/24-7 Penguins-Capitals- Road to the NHL Winter Classic/\",\"imdbnumber\":\"213041\",\"label\":\"24/7 Penguins/Capitals: Road to the NHL Winter Classic\",\"tvshowid\":1},{\"file\":\"smb://HOMESERVER/TV/30 Rock/\",\"imdbnumber\":\"79488\",\"label\":\"30 Rock\",\"tvshowid\":2},{\"file\":\"smb://HOMESERVER/TV/90210/\",\"imdbnumber\":\"82716\",\"label\":\"90210\",\"tvshowid\":5}]}}";

            var fakeHttp = Mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.PostCommand("localhost:8080", "xbmc", "xbmc", It.IsAny<string>()))
                .Returns(message);

            //Act
            var result = Mocker.Resolve<XbmcProvider>().GetTvShowsJson("localhost:8080", "xbmc", "xbmc");

            //Assert
            Assert.AreEqual(5, result.Count);
            result.Should().Contain(s => s.ImdbNumber == 79488);
        }

        [Test]
        public void Notify_true()
        {
            //Setup
            WithStrictMocker();

            var header = "NzbDrone Test";
            var message = "Test Message!";

            var fakeConfig = Mocker.GetMock<ConfigProvider>();
            fakeConfig.SetupGet(s => s.XbmcHosts).Returns("localhost:8080");

            //var fakeUdpProvider = Mocker.GetMock<EventClient>();
            var fakeEventClient = Mocker.GetMock<EventClientProvider>();
            fakeEventClient.Setup(s => s.SendNotification(header, message, IconType.Jpeg, "NzbDrone.jpg", "localhost")).Returns(true);

            //Act
            Mocker.Resolve<XbmcProvider>().Notify(header, message);

            //Assert
            Mocker.VerifyAllMocks();
        }

        [Test]
        public void SendCommand()
        {
            //Setup
            WithStrictMocker();

            var host = "localhost:8080";
            var command = "ExecBuiltIn(CleanLibrary(video))";
            var username = "xbmc";
            var password = "xbmc";

            var url = String.Format("http://localhost:8080/xbmcCmds/xbmcHttp?command=ExecBuiltIn(CleanLibrary(video))");

            //var fakeUdpProvider = Mocker.GetMock<EventClient>();
            var fakeHttp = Mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.DownloadString(url, username, password)).Returns("Ok\n");

            //Act
            var result = Mocker.Resolve<XbmcProvider>().SendCommand(host, command, username, username);

            //Assert
            Mocker.VerifyAllMocks();
            Assert.AreEqual("Ok\n", result);
        }

        [Test]
        public void GetXbmcSeriesPath_true()
        {
            //Setup
            WithStrictMocker();

            var queryResult = @"<xml><record><field>smb://xbmc:xbmc@HOMESERVER/TV/30 Rock/</field></record></xml>";

            var host = "localhost:8080";
            var username = "xbmc";
            var password = "xbmc";

            var setResponseUrl = "http://localhost:8080/xbmcCmds/xbmcHttp?command=SetResponseFormat(webheader;false;webfooter;false;header;<xml>;footer;</xml>;opentag;<tag>;closetag;</tag>;closefinaltag;false)";
            var resetResponseUrl = "http://localhost:8080/xbmcCmds/xbmcHttp?command=SetResponseFormat()";
            var query = String.Format("http://localhost:8080/xbmcCmds/xbmcHttp?command=QueryVideoDatabase(select path.strPath from path, tvshow, tvshowlinkpath where tvshow.c12 = 79488 and tvshowlinkpath.idShow = tvshow.idShow and tvshowlinkpath.idPath = path.idPath)");


            //var fakeUdpProvider = Mocker.GetMock<EventClient>();
            var fakeHttp = Mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.DownloadString(setResponseUrl, username, password)).Returns("<xml><tag>OK</xml>");
            fakeHttp.Setup(s => s.DownloadString(resetResponseUrl, username, password)).Returns(@"<html>
                                                                                                    <li>OK
                                                                                                    </html>");
            fakeHttp.Setup(s => s.DownloadString(query, username, password)).Returns(queryResult);

            //Act
            var result = Mocker.Resolve<XbmcProvider>().GetXbmcSeriesPath(host, 79488, username, username);

            //Assert
            Mocker.VerifyAllMocks();
            Assert.AreEqual("smb://xbmc:xbmc@HOMESERVER/TV/30 Rock/", result);
        }

        [Test]
        public void GetXbmcSeriesPath_false()
        {
            //Setup
            WithStrictMocker();

            var queryResult = @"<xml></xml>";

            var host = "localhost:8080";
            var username = "xbmc";
            var password = "xbmc";

            var setResponseUrl = "http://localhost:8080/xbmcCmds/xbmcHttp?command=SetResponseFormat(webheader;false;webfooter;false;header;<xml>;footer;</xml>;opentag;<tag>;closetag;</tag>;closefinaltag;false)";
            var resetResponseUrl = "http://localhost:8080/xbmcCmds/xbmcHttp?command=SetResponseFormat()";
            var query = String.Format("http://localhost:8080/xbmcCmds/xbmcHttp?command=QueryVideoDatabase(select path.strPath from path, tvshow, tvshowlinkpath where tvshow.c12 = 79488 and tvshowlinkpath.idShow = tvshow.idShow and tvshowlinkpath.idPath = path.idPath)");


            //var fakeUdpProvider = Mocker.GetMock<EventClient>();
            var fakeHttp = Mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.DownloadString(setResponseUrl, username, password)).Returns("<xml><tag>OK</xml>");
            fakeHttp.Setup(s => s.DownloadString(resetResponseUrl, username, password)).Returns(@"<html>
                                                                                                    <li>OK
                                                                                                    </html>");
            fakeHttp.Setup(s => s.DownloadString(query, username, password)).Returns(queryResult);

            //Act
            var result = Mocker.Resolve<XbmcProvider>().GetXbmcSeriesPath(host, 79488, username, username);

            //Assert
            Mocker.VerifyAllMocks();
            Assert.AreEqual("", result);
        }

        [Test]
        public void GetXbmcSeriesPath_special_characters()
        {
            //Setup
            WithStrictMocker();

            var queryResult = @"<xml><record><field>smb://xbmc:xbmc@HOMESERVER/TV/Law & Order- Special Victims Unit/</field></record></xml>";

            var host = "localhost:8080";
            var username = "xbmc";
            var password = "xbmc";

            var setResponseUrl = "http://localhost:8080/xbmcCmds/xbmcHttp?command=SetResponseFormat(webheader;false;webfooter;false;header;<xml>;footer;</xml>;opentag;<tag>;closetag;</tag>;closefinaltag;false)";
            var resetResponseUrl = "http://localhost:8080/xbmcCmds/xbmcHttp?command=SetResponseFormat()";
            var query = String.Format("http://localhost:8080/xbmcCmds/xbmcHttp?command=QueryVideoDatabase(select path.strPath from path, tvshow, tvshowlinkpath where tvshow.c12 = 79488 and tvshowlinkpath.idShow = tvshow.idShow and tvshowlinkpath.idPath = path.idPath)");


            //var fakeUdpProvider = Mocker.GetMock<EventClient>();
            var fakeHttp = Mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.DownloadString(setResponseUrl, username, password)).Returns("<xml><tag>OK</xml>");
            fakeHttp.Setup(s => s.DownloadString(resetResponseUrl, username, password)).Returns(@"<html>
                                                                                                    <li>OK
                                                                                                    </html>");
            fakeHttp.Setup(s => s.DownloadString(query, username, password)).Returns(queryResult);

            //Act
            var result = Mocker.Resolve<XbmcProvider>().GetXbmcSeriesPath(host, 79488, username, username);

            //Assert
            Mocker.VerifyAllMocks();
            result.Should().Be("smb://xbmc:xbmc@HOMESERVER/TV/Law & Order- Special Victims Unit/");
        }

        [Test]
        public void Clean()
        {
            //Setup
            WithStrictMocker();

            var fakeConfig = Mocker.GetMock<ConfigProvider>();
            fakeConfig.SetupGet(s => s.XbmcHosts).Returns("localhost:8080");

            var fakeEventClient = Mocker.GetMock<EventClientProvider>();
            fakeEventClient.Setup(s => s.SendAction("localhost", ActionType.ExecBuiltin, "ExecBuiltIn(CleanLibrary(video))")).Returns(true);

            //Act
            Mocker.Resolve<XbmcProvider>().Clean();

            //Assert
            Mocker.VerifyAllMocks();
        }

        [Test]
        public void UpdateWithHttp_Single()
        {
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

            var fakeHttp = Mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.DownloadString(queryUrl, username, password)).Returns(queryResult);
            fakeHttp.Setup(s => s.DownloadString(url, username, password));

            //Act
            Mocker.Resolve<XbmcProvider>().UpdateWithHttp(fakeSeries, host, username, password);

            //Assert
            Mocker.VerifyAllMocks();
        }

        [Test]
        public void UpdateWithHttp_All()
        {
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

            var fakeHttp = Mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.DownloadString(queryUrl, username, password)).Returns(queryResult);
            fakeHttp.Setup(s => s.DownloadString(url, username, password));

            //Act
            Mocker.Resolve<XbmcProvider>().UpdateWithHttp(fakeSeries, host, username, password);

            //Assert
            Mocker.VerifyAllMocks();
        }

        [Test]
        public void UpdateWithJson_Single()
        {
            //Setup
            

            var host = "localhost:8080";
            var username = "xbmc";
            var password = "xbmc";
            var serializedQuery = "{\"jsonrpc\":\"2.0\",\"method\":\"VideoLibrary.GetTvShows\",\"params\":{\"fields\":[\"file\",\"imdbnumber\"]},\"id\":10}";
            var tvshows = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"limits\":{\"end\":5,\"start\":0,\"total\":5},\"tvshows\":[{\"file\":\"smb://HOMESERVER/TV/7th Heaven/\",\"imdbnumber\":\"73928\",\"label\":\"7th Heaven\",\"tvshowid\":3},{\"file\":\"smb://HOMESERVER/TV/8 Simple Rules/\",\"imdbnumber\":\"78461\",\"label\":\"8 Simple Rules\",\"tvshowid\":4},{\"file\":\"smb://HOMESERVER/TV/24-7 Penguins-Capitals- Road to the NHL Winter Classic/\",\"imdbnumber\":\"213041\",\"label\":\"24/7 Penguins/Capitals: Road to the NHL Winter Classic\",\"tvshowid\":1},{\"file\":\"smb://HOMESERVER/TV/30 Rock/\",\"imdbnumber\":\"79488\",\"label\":\"30 Rock\",\"tvshowid\":2},{\"file\":\"smb://HOMESERVER/TV/90210/\",\"imdbnumber\":\"82716\",\"label\":\"90210\",\"tvshowid\":5}]}}";

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 79488)
                .With(s => s.Title = "30 Rock")
                .Build();

            var fakeHttp = Mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.PostCommand(host, username, password, serializedQuery))
                .Returns(tvshows);

            var fakeEventClient = Mocker.GetMock<EventClientProvider>();
            fakeEventClient.Setup(s => s.SendAction("localhost", ActionType.ExecBuiltin, "ExecBuiltIn(UpdateLibrary(video,smb://HOMESERVER/TV/30 Rock/))"));

            //Act
            Mocker.Resolve<XbmcProvider>().UpdateWithJson(fakeSeries, host, username, password);

            //Assert
            Mocker.VerifyAllMocks();
        }

        [Test]
        public void UpdateWithJson_All()
        {
            //Setup
            

            var host = "localhost:8080";
            var username = "xbmc";
            var password = "xbmc";
            var serializedQuery = "{\"jsonrpc\":\"2.0\",\"method\":\"VideoLibrary.GetTvShows\",\"params\":{\"fields\":[\"file\",\"imdbnumber\"]},\"id\":10}";
            var tvshows = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"limits\":{\"end\":5,\"start\":0,\"total\":5},\"tvshows\":[{\"file\":\"smb://HOMESERVER/TV/7th Heaven/\",\"imdbnumber\":\"73928\",\"label\":\"7th Heaven\",\"tvshowid\":3},{\"file\":\"smb://HOMESERVER/TV/8 Simple Rules/\",\"imdbnumber\":\"78461\",\"label\":\"8 Simple Rules\",\"tvshowid\":4},{\"file\":\"smb://HOMESERVER/TV/24-7 Penguins-Capitals- Road to the NHL Winter Classic/\",\"imdbnumber\":\"213041\",\"label\":\"24/7 Penguins/Capitals: Road to the NHL Winter Classic\",\"tvshowid\":1},{\"file\":\"smb://HOMESERVER/TV/90210/\",\"imdbnumber\":\"82716\",\"label\":\"90210\",\"tvshowid\":5}]}}";

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 79488)
                .With(s => s.Title = "30 Rock")
                .Build();

            var fakeHttp = Mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.PostCommand(host, username, password, serializedQuery))
                .Returns(tvshows);

            var fakeEventClient = Mocker.GetMock<EventClientProvider>();
            fakeEventClient.Setup(s => s.SendAction("localhost", ActionType.ExecBuiltin, "ExecBuiltIn(UpdateLibrary(video))"));

            //Act
            Mocker.Resolve<XbmcProvider>().UpdateWithJson(fakeSeries, host, username, password);

            //Assert
            Mocker.VerifyAllMocks();
        }
    }
}