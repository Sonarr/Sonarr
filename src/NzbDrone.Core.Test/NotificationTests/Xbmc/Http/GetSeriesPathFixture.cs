using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Notifications.Xbmc;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.NotificationTests.Xbmc.Http
{
    [TestFixture]
    public class GetSeriesPathFixture : CoreTest<HttpApiProvider>
    {
        private XbmcSettings _settings;
        private Series _series;

        [SetUp]
        public void Setup()
        {
            _settings = new XbmcSettings
            {
                Host = "localhost",
                Port = 8080,
                Username = "xbmc",
                Password = "xbmc",
                AlwaysUpdate = false,
                CleanLibrary = false,
                UpdateLibrary = true
            };

            _series = new Series
            {
                TvdbId = 79488,
                Title = "30 Rock"
            };

            const string setResponseUrl = "http://localhost:8080/xbmcCmds/xbmcHttp?command=SetResponseFormat(webheader;false;webfooter;false;header;<xml>;footer;</xml>;opentag;<tag>;closetag;</tag>;closefinaltag;false)";
            const string resetResponseUrl = "http://localhost:8080/xbmcCmds/xbmcHttp?command=SetResponseFormat()";

            Mocker.GetMock<IHttpProvider>()
                  .Setup(s => s.DownloadString(setResponseUrl, _settings.Username, _settings.Password))
                  .Returns("<xml><tag>OK</xml>");

            Mocker.GetMock<IHttpProvider>()
                  .Setup(s => s.DownloadString(resetResponseUrl, _settings.Username, _settings.Password))
                  .Returns(@"<html>
                             <li>OK
                             </html>");
        }

        [Test]
        public void should_get_series_path()
        {
            const string queryResult = @"<xml><record><field>smb://xbmc:xbmc@HOMESERVER/TV/30 Rock/</field></record></xml>";
            var query = string.Format("http://localhost:8080/xbmcCmds/xbmcHttp?command=QueryVideoDatabase(select path.strPath from path, tvshow, tvshowlinkpath where tvshow.c12 = 79488 and tvshowlinkpath.idShow = tvshow.idShow and tvshowlinkpath.idPath = path.idPath)");

            Mocker.GetMock<IHttpProvider>()
                  .Setup(s => s.DownloadString(query, _settings.Username, _settings.Password))
                  .Returns(queryResult);

            Subject.GetSeriesPath(_settings, _series)
                   .Should().Be("smb://xbmc:xbmc@HOMESERVER/TV/30 Rock/");
        }

        [Test]
        public void should_get_null_for_series_path()
        {
            const string queryResult = @"<xml></xml>";
            var query = string.Format("http://localhost:8080/xbmcCmds/xbmcHttp?command=QueryVideoDatabase(select path.strPath from path, tvshow, tvshowlinkpath where tvshow.c12 = 79488 and tvshowlinkpath.idShow = tvshow.idShow and tvshowlinkpath.idPath = path.idPath)");

            Mocker.GetMock<IHttpProvider>()
                  .Setup(s => s.DownloadString(query, _settings.Username, _settings.Password))
                  .Returns(queryResult);


            Subject.GetSeriesPath(_settings, _series)
                   .Should().BeNull();
        }

        [Test]
        public void should_get_series_path_with_special_characters_in_it()
        {
            const string queryResult = @"<xml><record><field>smb://xbmc:xbmc@HOMESERVER/TV/Law & Order- Special Victims Unit/</field></record></xml>";
            var query = string.Format("http://localhost:8080/xbmcCmds/xbmcHttp?command=QueryVideoDatabase(select path.strPath from path, tvshow, tvshowlinkpath where tvshow.c12 = 79488 and tvshowlinkpath.idShow = tvshow.idShow and tvshowlinkpath.idPath = path.idPath)");

            Mocker.GetMock<IHttpProvider>()
                  .Setup(s => s.DownloadString(query, _settings.Username, _settings.Password))
                  .Returns(queryResult);


            Subject.GetSeriesPath(_settings, _series)
                   .Should().Be("smb://xbmc:xbmc@HOMESERVER/TV/Law & Order- Special Victims Unit/");
        }
    }
}
