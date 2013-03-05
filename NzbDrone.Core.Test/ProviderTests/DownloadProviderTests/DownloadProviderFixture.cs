using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Download.Clients.Sabnzbd;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Test.Framework;

// ReSharper disable InconsistentNaming

namespace NzbDrone.Core.Test.ProviderTests.DownloadProviderTests
{
    [TestFixture]
    public class DownloadProviderFixture : CoreTest<DownloadProvider>
    {


        private void SetDownloadClient(DownloadClientType clientType)
        {
            Mocker.GetMock<IConfigService>()
                 .Setup(c => c.DownloadClient)
                 .Returns(clientType);
        }

        private EpisodeParseResult SetupParseResult()
        {
            var episodes = Builder<Episode>.CreateListOfSize(2)
                            .TheFirst(1).With(s => s.Id = 12)
                            .TheNext(1).With(s => s.Id = 99)
                            .All().With(s => s.SeriesId = 5)
                            .Build().ToList();

            Mocker.GetMock<IEpisodeService>()
                    .Setup(c => c.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>())).Returns(episodes);

            return Builder<EpisodeParseResult>.CreateNew()
                .With(c => c.Quality = new QualityModel(Quality.DVD, false))
                .With(c => c.Series = Builder<Series>.CreateNew().Build())
                .With(c => c.EpisodeNumbers = new List<int> { 2 })
                .With(c => c.Episodes = episodes)
                .Build();
        }

        private void WithSuccessfullAdd()
        {
            Mocker.GetMock<SabProvider>()
                .Setup(s => s.DownloadNzb(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<bool>()))
                .Returns(true);

            Mocker.GetMock<BlackholeProvider>()
                .Setup(s => s.DownloadNzb(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<bool>()))
                .Returns(true);
        }

        private void WithFailedAdd()
        {
            Mocker.GetMock<SabProvider>()
                .Setup(s => s.DownloadNzb(It.IsAny<String>(), It.IsAny<String>(), false))
                .Returns(false);

            Mocker.GetMock<BlackholeProvider>()
                .Setup(s => s.DownloadNzb(It.IsAny<String>(), It.IsAny<String>(), false))
                .Returns(false);
        }

        [Test]
        public void Download_report_should_publish_on_grab_event()
        {
            WithSuccessfullAdd();
            SetDownloadClient(DownloadClientType.Sabnzbd);

            var parseResult = SetupParseResult();

            //Act
            Subject.DownloadReport(parseResult);


            //Assert
            Mocker.GetMock<SabProvider>()
                .Verify(s => s.DownloadNzb(It.IsAny<String>(), It.IsAny<String>(), true), Times.Once());

            Mocker.GetMock<BlackholeProvider>()
                .Verify(s => s.DownloadNzb(It.IsAny<String>(), It.IsAny<String>(), true), Times.Never());


            VerifyEventPublished<EpisodeGrabbedEvent>();
        }

        [TestCase(DownloadClientType.Sabnzbd)]
        [TestCase(DownloadClientType.Blackhole)]
        public void Download_report_should_not_publish_grabbed_event(DownloadClientType clientType)
        {
            WithFailedAdd();
            SetDownloadClient(clientType);

            var parseResult = SetupParseResult();

            Subject.DownloadReport(parseResult);


            VerifyEventNotPublished<EpisodeGrabbedEvent>();
        }

        [Test]
        public void should_return_sab_as_active_client()
        {
            SetDownloadClient(DownloadClientType.Sabnzbd);
            Subject.GetActiveDownloadClient().Should().BeAssignableTo<SabProvider>();
        }

        [Test]
        public void should_return_blackhole_as_active_client()
        {
            SetDownloadClient(DownloadClientType.Blackhole);
            Subject.GetActiveDownloadClient().Should().BeAssignableTo<BlackholeProvider>();
        }


    }
}