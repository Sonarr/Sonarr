using System;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Nzbget;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.NzbgetTests
{
    public class DownloadNzbFixture : CoreTest<Nzbget>
    {
        private const string _url = "http://www.nzbdrone.com";
        private const string _title = "30.Rock.S01E01.Pilot.720p.hdtv";
        private RemoteEpisode _remoteEpisode;

        [SetUp]
        public void Setup()
        {
            _remoteEpisode = new RemoteEpisode();
            _remoteEpisode.Release = new ReleaseInfo();
            _remoteEpisode.Release.Title = _title;
            _remoteEpisode.Release.DownloadUrl = _url;

            _remoteEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                      .All()
                                                      .With(e => e.AirDate = DateTime.Today.ToString(Episode.AIR_DATE_FORMAT))
                                                      .Build()
                                                      .ToList();

            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = new NzbgetSettings
            {
                Host = "localhost",
                Port = 6789,
                Username = "nzbget",
                Password = "pass",
                TvCategory = "tv",
                RecentTvPriority = (int)NzbgetPriority.High
            };
        }

        [Test]
        public void should_add_item_to_queue()
        {
            Mocker.GetMock<INzbgetProxy>()
                  .Setup(s => s.DownloadNzb(It.IsAny<Stream>(), It.IsAny<String>(), It.IsAny<String>(), It.IsAny<Int32>(), It.IsAny<NzbgetSettings>()))
                  .Returns("id");

            Subject.DownloadNzb(_remoteEpisode);

            Mocker.GetMock<INzbgetProxy>()
                  .Verify(v => v.DownloadNzb(It.IsAny<Stream>(), It.IsAny<String>(), It.IsAny<String>(), It.IsAny<Int32>(), It.IsAny<NzbgetSettings>()), Times.Once());
        }
    }
}
