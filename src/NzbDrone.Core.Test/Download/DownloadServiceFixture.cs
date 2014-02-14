using System;
using System.Linq;
using System.Net;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download
{
    [TestFixture]
    public class DownloadServiceFixture : CoreTest<DownloadService>
    {
        private RemoteEpisode _parseResult;

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IProvideDownloadClient>()
                  .Setup(c => c.GetDownloadClient()).Returns(Mocker.GetMock<IDownloadClient>().Object);

            var episodes = Builder<Episode>.CreateListOfSize(2)
                .TheFirst(1).With(s => s.Id = 12)
                .TheNext(1).With(s => s.Id = 99)
                .All().With(s => s.SeriesId = 5)
                .Build().ToList();

            _parseResult = Builder<RemoteEpisode>.CreateNew()
                   .With(c => c.Series = Builder<Series>.CreateNew().Build())
                   .With(c => c.Release = Builder<ReleaseInfo>.CreateNew().Build())
                   .With(c => c.Episodes = episodes)
                   .Build();
        }

        private void WithSuccessfulAdd()
        {
            Mocker.GetMock<IDownloadClient>()
                  .Setup(s => s.DownloadNzb(It.IsAny<RemoteEpisode>()));
        }

        private void WithFailedAdd()
        {
            Mocker.GetMock<IDownloadClient>()
                .Setup(s => s.DownloadNzb(It.IsAny<RemoteEpisode>()))
                .Throws(new WebException());
        }

        [Test]
        public void Download_report_should_publish_on_grab_event()
        {
            WithSuccessfulAdd();

            Subject.DownloadReport(_parseResult);

            VerifyEventPublished<EpisodeGrabbedEvent>();
        }

        [Test]
        public void Download_report_should_grab_using_client()
        {
            WithSuccessfulAdd();

            Subject.DownloadReport(_parseResult);

            Mocker.GetMock<IDownloadClient>()
                .Verify(s => s.DownloadNzb(It.IsAny<RemoteEpisode>()), Times.Once());
        }

        [Test]
        public void Download_report_should_not_publish_on_failed_grab_event()
        {
            WithFailedAdd();

            Assert.Throws<WebException>(() => Subject.DownloadReport(_parseResult));

            VerifyEventNotPublished<EpisodeGrabbedEvent>();
        }

        [Test]
        public void should_not_attempt_download_if_client_isnt_configure()
        {
            Mocker.GetMock<IProvideDownloadClient>()
                  .Setup(c => c.GetDownloadClient()).Returns((IDownloadClient)null);

            Subject.DownloadReport(_parseResult);

            Mocker.GetMock<IDownloadClient>().Verify(c => c.DownloadNzb(It.IsAny<RemoteEpisode>()), Times.Never());
            VerifyEventNotPublished<EpisodeGrabbedEvent>();

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
