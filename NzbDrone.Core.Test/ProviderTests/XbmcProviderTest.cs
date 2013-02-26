// ReSharper disable RedundantUsingDirective

using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Tv;
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
        private string EdenActivePlayers;

        private void WithNoActivePlayers()
        {
            EdenActivePlayers = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":[]}";
        }

        private void WithVideoPlayerActive()
        {
            EdenActivePlayers = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":[{\"playerid\":1,\"type\":\"video\"}]}";
        }

        private void WithAudioPlayerActive()
        {
            EdenActivePlayers = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":[{\"playerid\":1,\"type\":\"audio\"}]}";
        }

        private void WithPicturePlayerActive()
        {
            EdenActivePlayers = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":[{\"playerid\":1,\"type\":\"picture\"}]}";
        }

        private void WithAllPlayersActive()
        {
            EdenActivePlayers = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":[{\"playerid\":1,\"type\":\"audio\"},{\"playerid\":2,\"type\":\"picture\"},{\"playerid\":3,\"type\":\"video\"}]}";
        }

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
        public void GetJsonVersionIntOnly(int number)
        {
            var message = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"version\":" + number + "}}";

            var fakeHttp = Mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.PostCommand("localhost:8080", "xbmc", "xbmc", It.IsAny<string>()))
                .Returns(message);

            //Act
            var result = Mocker.Resolve<XbmcProvider>().GetJsonVersion("localhost:8080", "xbmc", "xbmc");

            //Assert
            result.Should().Be(new XbmcVersion(number));
        }

        [TestCase(5, 0, 0)]
        [TestCase(6, 0, 0)]
        [TestCase(6, 1, 0)]
        [TestCase(6, 0, 23)]
        [TestCase(0, 0, 0)]
        public void GetJsonVersionFrodo(int major, int minor, int patch)
        {
            var message = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"version\":{\"major\":" + major + ",\"minor\":" + minor + ",\"patch\":" + patch + "}}}";

            var fakeHttp = Mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.PostCommand("localhost:8080", "xbmc", "xbmc", It.IsAny<string>()))
                .Returns(message);

            //Act
            var result = Mocker.Resolve<XbmcProvider>().GetJsonVersion("localhost:8080", "xbmc", "xbmc");

            //Assert
            result.Should().Be(new XbmcVersion(major, minor, patch));
        }

        [Test]
        public void GetJsonVersion_error()
        {
            var message = "{\"error\":{\"code\":-32601,\"message\":\"Method not found.\"},\"id\":10,\"jsonrpc\":\"2.0\"}";

            var fakeHttp = Mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.PostCommand("localhost:8080", "xbmc", "xbmc", It.IsAny<string>()))
                .Returns(message);

            //Act
            var result = Mocker.Resolve<XbmcProvider>().GetJsonVersion("localhost:8080", "xbmc", "xbmc");

            //Assert
            result.Should().Be(new XbmcVersion(0));
        }

        [TestCase(false, false, false)]
        [TestCase(true, true, true)]
        [TestCase(true, false, false)]
        [TestCase(true, true, false)]
        [TestCase(false, true, false)]
        [TestCase(false, true, true)]
        [TestCase(false, false, true)]
        [TestCase(true, false, true)]
        public void GetActivePlayersDharma(bool audio, bool picture, bool video)
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
            var result = Mocker.Resolve<XbmcProvider>().GetActivePlayersDharma("localhost:8080", "xbmc", "xbmc");

            //Assert
            Assert.AreEqual(audio, result["audio"]);
            Assert.AreEqual(picture, result["picture"]);
            Assert.AreEqual(video, result["video"]);
        }

        [Test]
        public void GetActivePlayersEden_should_be_empty_when_no_active_players()
        {
            //Setup
            WithNoActivePlayers();

            var fakeHttp = Mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.PostCommand("localhost:8080", "xbmc", "xbmc", It.IsAny<string>()))
                .Returns(EdenActivePlayers);

            //Act
            var result = Mocker.Resolve<XbmcProvider>().GetActivePlayersEden("localhost:8080", "xbmc", "xbmc");

            //Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void GetActivePlayersEden_should_have_active_video_player()
        {
            //Setup
            WithVideoPlayerActive();

            var fakeHttp = Mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.PostCommand("localhost:8080", "xbmc", "xbmc", It.IsAny<string>()))
                .Returns(EdenActivePlayers);

            //Act
            var result = Mocker.Resolve<XbmcProvider>().GetActivePlayersEden("localhost:8080", "xbmc", "xbmc");

            //Assert
            result.Should().HaveCount(1);
            result.First().Type.Should().Be("video");
        }

        [Test]
        public void GetActivePlayersEden_should_have_active_audio_player()
        {
            //Setup
            WithAudioPlayerActive();

            var fakeHttp = Mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.PostCommand("localhost:8080", "xbmc", "xbmc", It.IsAny<string>()))
                .Returns(EdenActivePlayers);

            //Act
            var result = Mocker.Resolve<XbmcProvider>().GetActivePlayersEden("localhost:8080", "xbmc", "xbmc");

            //Assert
            result.Should().HaveCount(1);
            result.First().Type.Should().Be("audio");
        }

        [Test]
        public void GetActivePlayersEden_should_have_active_picture_player()
        {
            //Setup
            WithPicturePlayerActive();

            var fakeHttp = Mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.PostCommand("localhost:8080", "xbmc", "xbmc", It.IsAny<string>()))
                .Returns(EdenActivePlayers);

            //Act
            var result = Mocker.Resolve<XbmcProvider>().GetActivePlayersEden("localhost:8080", "xbmc", "xbmc");

            //Assert
            result.Should().HaveCount(1);
            result.First().Type.Should().Be("picture");
        }

        [Test]
        public void GetActivePlayersEden_should_have_all_players_active()
        {
            //Setup
            WithAllPlayersActive();

            var fakeHttp = Mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.PostCommand("localhost:8080", "xbmc", "xbmc", It.IsAny<string>()))
                .Returns(EdenActivePlayers);

            //Act
            var result = Mocker.Resolve<XbmcProvider>().GetActivePlayersEden("localhost:8080", "xbmc", "xbmc");

            //Assert
            result.Should().HaveCount(3);
            result.Select(a => a.PlayerId).Distinct().Should().HaveCount(3);
            result.Select(a => a.Type).Distinct().Should().HaveCount(3);
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

            var fakeConfig = Mocker.GetMock<IConfigService>();
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

            var fakeConfig = Mocker.GetMock<IConfigService>();
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
                .With(s => s.Id = 79488)
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
                .With(s => s.Id = 79488)
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
        public void UpdateWithJsonBuiltIn_Single()
        {
            //Setup
            

            var host = "localhost:8080";
            var username = "xbmc";
            var password = "xbmc";
            var expectedJson = "{\"jsonrpc\":\"2.0\",\"method\":\"VideoLibrary.GetTvShows\",\"params\":{\"properties\":[\"file\",\"imdbnumber\"]},\"id\":10}";
            var tvshows = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"limits\":{\"end\":5,\"start\":0,\"total\":5},\"tvshows\":[{\"file\":\"smb://HOMESERVER/TV/7th Heaven/\",\"imdbnumber\":\"73928\",\"label\":\"7th Heaven\",\"tvshowid\":3},{\"file\":\"smb://HOMESERVER/TV/8 Simple Rules/\",\"imdbnumber\":\"78461\",\"label\":\"8 Simple Rules\",\"tvshowid\":4},{\"file\":\"smb://HOMESERVER/TV/24-7 Penguins-Capitals- Road to the NHL Winter Classic/\",\"imdbnumber\":\"213041\",\"label\":\"24/7 Penguins/Capitals: Road to the NHL Winter Classic\",\"tvshowid\":1},{\"file\":\"smb://HOMESERVER/TV/30 Rock/\",\"imdbnumber\":\"79488\",\"label\":\"30 Rock\",\"tvshowid\":2},{\"file\":\"smb://HOMESERVER/TV/90210/\",\"imdbnumber\":\"82716\",\"label\":\"90210\",\"tvshowid\":5}]}}";

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.Id = 79488)
                .With(s => s.Title = "30 Rock")
                .Build();

            var fakeHttp = Mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.PostCommand(host, username, password, It.Is<string>(e => e.Replace(" ", "").Replace("\r\n", "").Replace("\t", "") == expectedJson.Replace(" ", ""))))
                .Returns(tvshows);

            var command = "ExecBuiltIn(UpdateLibrary(video,smb://HOMESERVER/TV/30 Rock/))";
            var url = String.Format("http://{0}/xbmcCmds/xbmcHttp?command={1}", host, command);

            fakeHttp.Setup(s => s.DownloadString(url, username, password)).Returns("<html><li>OK</html>");

            //Act
            var result = Mocker.Resolve<XbmcProvider>().UpdateWithJsonExecBuiltIn(fakeSeries, host, username, password);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void UpdateWithJsonBuiltIn_All()
        {
            //Setup
            

            var host = "localhost:8080";
            var username = "xbmc";
            var password = "xbmc";
            var expectedJson = "{\"jsonrpc\":\"2.0\",\"method\":\"VideoLibrary.GetTvShows\",\"params\":{\"properties\":[\"file\",\"imdbnumber\"]},\"id\":10}";
            var tvshows = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"limits\":{\"end\":5,\"start\":0,\"total\":5},\"tvshows\":[{\"file\":\"smb://HOMESERVER/TV/7th Heaven/\",\"imdbnumber\":\"73928\",\"label\":\"7th Heaven\",\"tvshowid\":3},{\"file\":\"smb://HOMESERVER/TV/8 Simple Rules/\",\"imdbnumber\":\"78461\",\"label\":\"8 Simple Rules\",\"tvshowid\":4},{\"file\":\"smb://HOMESERVER/TV/24-7 Penguins-Capitals- Road to the NHL Winter Classic/\",\"imdbnumber\":\"213041\",\"label\":\"24/7 Penguins/Capitals: Road to the NHL Winter Classic\",\"tvshowid\":1},{\"file\":\"smb://HOMESERVER/TV/90210/\",\"imdbnumber\":\"82716\",\"label\":\"90210\",\"tvshowid\":5}]}}";

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.Id = 79488)
                .With(s => s.Title = "30 Rock")
                .Build();

            var fakeHttp = Mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.PostCommand(host, username, password, It.Is<string>(e => e.Replace(" ", "").Replace("\r\n", "").Replace("\t", "") == expectedJson.Replace(" ", ""))))
                .Returns(tvshows);

            var command = "ExecBuiltIn(UpdateLibrary(video))";
            var url = String.Format("http://{0}/xbmcCmds/xbmcHttp?command={1}", host, command);

            fakeHttp.Setup(s => s.DownloadString(url, username, password)).Returns("<html><li>OK</html>");

            //var fakeEventClient = Mocker.GetMock<EventClientProvider>();
            //fakeEventClient.Setup(s => s.SendAction("localhost", ActionType.ExecBuiltin, "ExecBuiltIn(UpdateLibrary(video))"));

            //Act
            var result = Mocker.Resolve<XbmcProvider>().UpdateWithJsonExecBuiltIn(fakeSeries, host, username, password);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void UpdateWithJsonVideoLibraryScan_Single()
        {
            var host = "localhost:8080";
            var username = "xbmc";
            var password = "xbmc";
            var expectedJson = "{\"jsonrpc\":\"2.0\",\"method\":\"VideoLibrary.GetTvShows\",\"params\":{\"properties\":[\"file\",\"imdbnumber\"]},\"id\":10}";
            var tvshows = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"limits\":{\"end\":5,\"start\":0,\"total\":5},\"tvshows\":[{\"file\":\"smb://HOMESERVER/TV/7th Heaven/\",\"imdbnumber\":\"73928\",\"label\":\"7th Heaven\",\"tvshowid\":3},{\"file\":\"smb://HOMESERVER/TV/8 Simple Rules/\",\"imdbnumber\":\"78461\",\"label\":\"8 Simple Rules\",\"tvshowid\":4},{\"file\":\"smb://HOMESERVER/TV/24-7 Penguins-Capitals- Road to the NHL Winter Classic/\",\"imdbnumber\":\"213041\",\"label\":\"24/7 Penguins/Capitals: Road to the NHL Winter Classic\",\"tvshowid\":1},{\"file\":\"smb://HOMESERVER/TV/30 Rock/\",\"imdbnumber\":\"79488\",\"label\":\"30 Rock\",\"tvshowid\":2},{\"file\":\"smb://HOMESERVER/TV/90210/\",\"imdbnumber\":\"82716\",\"label\":\"90210\",\"tvshowid\":5}]}}";

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.Id = 79488)
                .With(s => s.Title = "30 Rock")
                .Build();

            var fakeHttp = Mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.PostCommand(host, username, password, It.Is<string>(e => e.Replace(" ", "").Replace("\r\n", "").Replace("\t", "") == expectedJson.Replace(" ", ""))))
                .Returns(tvshows);

            fakeHttp.Setup(s => s.PostCommand(host, username, password, It.Is<String>(
                e => e.Replace(" ", "")
                      .Replace("\r\n", "")
                      .Replace("\t", "")
                      .Contains("\"params\":{\"directory\":\"smb://HOMESERVER/TV/30Rock/\"}"))))
                      .Returns("{\"id\":55,\"jsonrpc\":\"2.0\",\"result\":\"OK\"}");

            //Act
            var result = Mocker.Resolve<XbmcProvider>().UpdateWithJsonVideoLibraryScan(fakeSeries, host, username, password);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void UpdateWithJsonVideoLibraryScan_All()
        {
            var host = "localhost:8080";
            var username = "xbmc";
            var password = "xbmc";
            var expectedJson = "{\"jsonrpc\":\"2.0\",\"method\":\"VideoLibrary.GetTvShows\",\"params\":{\"properties\":[\"file\",\"imdbnumber\"]},\"id\":10}";
            var tvshows = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"limits\":{\"end\":5,\"start\":0,\"total\":5},\"tvshows\":[{\"file\":\"smb://HOMESERVER/TV/7th Heaven/\",\"imdbnumber\":\"73928\",\"label\":\"7th Heaven\",\"tvshowid\":3},{\"file\":\"smb://HOMESERVER/TV/8 Simple Rules/\",\"imdbnumber\":\"78461\",\"label\":\"8 Simple Rules\",\"tvshowid\":4},{\"file\":\"smb://HOMESERVER/TV/24-7 Penguins-Capitals- Road to the NHL Winter Classic/\",\"imdbnumber\":\"213041\",\"label\":\"24/7 Penguins/Capitals: Road to the NHL Winter Classic\",\"tvshowid\":1},{\"file\":\"smb://HOMESERVER/TV/90210/\",\"imdbnumber\":\"82716\",\"label\":\"90210\",\"tvshowid\":5}]}}";

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.Id = 79488)
                .With(s => s.Title = "30 Rock")
                .Build();

            var fakeHttp = Mocker.GetMock<HttpProvider>();
            fakeHttp.Setup(s => s.PostCommand(host, username, password, It.Is<string>(e => e.Replace(" ", "").Replace("\r\n", "").Replace("\t", "") == expectedJson.Replace(" ", ""))))
                .Returns(tvshows);

            fakeHttp.Setup(s => s.PostCommand(host, username, password, It.Is<String>(
                e => !e.Replace(" ", "")
                      .Replace("\r\n", "")
                      .Replace("\t", "")
                      .Contains("\"params\":{\"directory\":\"smb://HOMESERVER/TV/30Rock/\"}"))))
                      .Returns("{\"id\":55,\"jsonrpc\":\"2.0\",\"result\":\"OK\"}");

            //Act
            var result = Mocker.Resolve<XbmcProvider>().UpdateWithJsonVideoLibraryScan(fakeSeries, host, username, password);

            //Assert
            result.Should().BeTrue();
        }
    }
}