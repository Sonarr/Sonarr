using System;
using AutoMoq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

// ReSharper disable InconsistentNaming

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    public class DownloadProviderTest : TestBase
    {
        [Test]
        public void Download_report_should_send_to_sab_add_to_history_mark_as_grabbed()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var parseResult = Builder<EpisodeParseResult>.CreateNew()
                .With(c => c.Quality = new Quality(QualityTypes.DVD, false))
                .Build();

            var episodes = Builder<Episode>.CreateListOfSize(2)
                                            .TheFirst(1).Has(s => s.EpisodeId = 12)
                                            .AndTheNext(1).Has(s => s.EpisodeId = 99)
                                            .All().Has(s => s.SeriesId = 5)
                                            .Build();


            const string sabTitle = "My fake sab title";
            mocker.GetMock<SabProvider>()
                .Setup(s => s.IsInQueue(It.IsAny<String>()))
                .Returns(false);

            mocker.GetMock<SabProvider>()
                .Setup(s => s.AddByUrl(parseResult.NzbUrl, sabTitle))
                .Returns(true);

            mocker.GetMock<SabProvider>()
                .Setup(s => s.GetSabTitle(parseResult))
                .Returns(sabTitle);

            mocker.GetMock<HistoryProvider>()
                .Setup(s => s.Add(It.Is<History>(h => h.EpisodeId == 12 && h.SeriesId == 5)));
            mocker.GetMock<HistoryProvider>()
                .Setup(s => s.Add(It.Is<History>(h => h.EpisodeId == 99 && h.SeriesId == 5)));

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>(), false)).Returns(episodes);

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.MarkEpisodeAsFetched(12));

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.MarkEpisodeAsFetched(99));

            mocker.GetMock<ExternalNotificationProvider>()
                .Setup(c => c.OnGrab(It.IsAny<string>()));

            mocker.Resolve<DownloadProvider>().DownloadReport(parseResult);

            mocker.VerifyAllMocks();
        }
    }
}