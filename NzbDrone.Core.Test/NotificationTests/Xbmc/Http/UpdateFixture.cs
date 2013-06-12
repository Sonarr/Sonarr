using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Notifications.Xbmc;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.NotificationTests.Xbmc.Http
{
    [TestFixture]
    public class UpdateFixture : CoreTest<HttpApiProvider>
    {
        private XbmcSettings _settings;
        private string _seriesQueryUrl = "http://localhost:8080/xbmcCmds/xbmcHttp?command=QueryVideoDatabase(select path.strPath from path, tvshow, tvshowlinkpath where tvshow.c12 = 79488 and tvshowlinkpath.idShow = tvshow.idShow and tvshowlinkpath.idPath = path.idPath)";
        private Series _fakeSeries;

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

            _fakeSeries = Builder<Series>.CreateNew()
                                         .With(s => s.TvdbId = 79488)
                                         .With(s => s.Title = "30 Rock")
                                         .Build();
        }

        private void WithSeriesPath()
        {
            Mocker.GetMock<IHttpProvider>()
                  .Setup(s => s.DownloadString(_seriesQueryUrl, _settings.Username, _settings.Password))
                  .Returns("<xml><record><field>smb://xbmc:xbmc@HOMESERVER/TV/30 Rock/</field></record></xml>");
        }

        private void WithoutSeriesPath()
        {
            Mocker.GetMock<IHttpProvider>()
                  .Setup(s => s.DownloadString(_seriesQueryUrl, _settings.Username, _settings.Password))
                  .Returns("<xml></xml>");
        }

        [Test]
        public void should_update_using_series_path()
        {
            WithSeriesPath();
            const string url = "http://localhost:8080/xbmcCmds/xbmcHttp?command=ExecBuiltIn(UpdateLibrary(video,smb://xbmc:xbmc@HOMESERVER/TV/30 Rock/))";

            Mocker.GetMock<IHttpProvider>().Setup(s => s.DownloadString(url, _settings.Username, _settings.Password));

            Subject.Update(_settings, _fakeSeries);
            Mocker.VerifyAllMocks();
        }

        [Test]
        public void should_update_all_paths_when_series_path_not_found()
        {
            WithoutSeriesPath();
            const string url = "http://localhost:8080/xbmcCmds/xbmcHttp?command=ExecBuiltIn(UpdateLibrary(video))";

            Mocker.GetMock<IHttpProvider>().Setup(s => s.DownloadString(url, _settings.Username, _settings.Password));

            Subject.Update(_settings, _fakeSeries);
            Mocker.VerifyAllMocks();
        }
    }
}
