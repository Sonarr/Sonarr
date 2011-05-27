using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using AutoMoq;
using FizzWare.NBuilder;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Moq;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Jobs;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class EpisodeSearchJobTest : TestBase
    {
        [Test]
        public void ParseResult_should_return_after_match()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(5)
                .Build();

            var episode = Builder<Episode>.CreateNew().Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<InventoryProvider>()
                .Setup(c => c.IsNeeded(It.IsAny<EpisodeParseResult>())).Returns(true)
                .AtMostOnce();

            mocker.GetMock<DownloadProvider>()
                .Setup(c => c.DownloadReport(It.IsAny<EpisodeParseResult>())).Returns(true)
            .AtMostOnce();

            mocker.Resolve<EpisodeSearchJob>().ProcessResults(new ProgressNotification("Test"), episode, parseResults);


            mocker.VerifyAllMocks();
        }


        [Test]
        public void higher_quality_should_be_called_first()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(2)
                .WhereTheFirst(1).Has(c => c.Quality = QualityTypes.Bluray1080p)
                .AndTheNext(1).Has(c => c.Quality = QualityTypes.DVD)
                .Build();

            var episode = Builder<Episode>.CreateNew().Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<InventoryProvider>()
                .Setup(c => c.IsNeeded(parseResults[0])).Returns(true)
                .AtMostOnce();

            mocker.GetMock<DownloadProvider>()
                .Setup(c => c.DownloadReport(parseResults[0])).Returns(true)
            .AtMostOnce();

            mocker.Resolve<EpisodeSearchJob>().ProcessResults(new ProgressNotification("Test"), episode, parseResults);


            mocker.VerifyAllMocks();
        }


        [Test]
        public void when_same_quality_proper_should_be_called_first()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(20)
                .WhereAll().Have(c => c.Quality = QualityTypes.DVD)
                .And(c => c.Proper = false)
               .WhereRandom(1).Has(c => c.Proper = true)
                .Build();

            Assert.Count(1, parseResults.Where(c => c.Proper));

            var episode = Builder<Episode>.CreateNew().Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<InventoryProvider>()
                .Setup(c => c.IsNeeded(It.Is<EpisodeParseResult>(p => p.Proper))).Returns(true)
                .AtMostOnce();

            mocker.GetMock<DownloadProvider>()
                .Setup(c => c.DownloadReport(It.Is<EpisodeParseResult>(p => p.Proper))).Returns(true)
            .AtMostOnce();

            mocker.Resolve<EpisodeSearchJob>().ProcessResults(new ProgressNotification("Test"), episode, parseResults);


            mocker.VerifyAllMocks();
        }


        [Test]
        public void when_not_needed_should_check_the_rest()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(4)
                .Build();

            var episode = Builder<Episode>.CreateNew().Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<InventoryProvider>()
                .Setup(c => c.IsNeeded(It.IsAny<EpisodeParseResult>())).Returns(false);

            mocker.Resolve<EpisodeSearchJob>().ProcessResults(new ProgressNotification("Test"), episode, parseResults);


            mocker.VerifyAllMocks();
            mocker.GetMock<InventoryProvider>().Verify(c => c.IsNeeded(It.IsAny<EpisodeParseResult>()), Times.Exactly(4));

            ExceptionVerification.ExcpectedWarns(1);
        }


        [Test]
        public void failed_is_neede_should_check_the_rest()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(4)
                .Build();

            var episode = Builder<Episode>.CreateNew().Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<InventoryProvider>()
                .Setup(c => c.IsNeeded(It.IsAny<EpisodeParseResult>())).Throws(new Exception());

            mocker.Resolve<EpisodeSearchJob>().ProcessResults(new ProgressNotification("Test"), episode, parseResults);


            mocker.VerifyAllMocks();
            mocker.GetMock<InventoryProvider>().Verify(c => c.IsNeeded(It.IsAny<EpisodeParseResult>()), Times.Exactly(4));

            ExceptionVerification.ExcpectedErrors(4);
            ExceptionVerification.ExcpectedWarns(1);
        }
    }
}
