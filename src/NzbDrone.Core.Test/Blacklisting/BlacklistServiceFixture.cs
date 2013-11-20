using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.Download;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Blacklisting
{
    [TestFixture]
    public class BlacklistServiceFixture : CoreTest<BlacklistService>
    {
        private DownloadFailedEvent _event;

        [SetUp]
        public void Setup()
        {
            _event = new DownloadFailedEvent
                     {
                         SeriesId = 12345,
                         EpisodeIds = new List<int> {1},
                         Quality = new QualityModel(Quality.Bluray720p),
                         SourceTitle = "series.title.s01e01",
                         DownloadClient = "SabnzbdClient",
                         DownloadClientId = "Sabnzbd_nzo_2dfh73k"
                     };
        }

        [Test]
        public void should_trigger_redownload()
        {
            Subject.Handle(_event);

            Mocker.GetMock<IRedownloadFailedDownloads>()
                .Verify(v => v.Redownload(_event.SeriesId, _event.EpisodeIds), Times.Once());
        }

        [Test]
        public void should_add_to_repository()
        {
            Subject.Handle(_event);

            Mocker.GetMock<IBlacklistRepository>()
                .Verify(v => v.Insert(It.Is<Blacklist>(b => b.EpisodeIds == _event.EpisodeIds)), Times.Once());
        }
    }
}
