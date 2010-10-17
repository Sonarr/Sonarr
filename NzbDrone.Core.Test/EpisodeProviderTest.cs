using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using FizzWare.NBuilder;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Moq;
using Ninject;
using Ninject.Moq;
using NzbDrone.Core.Providers;
using SubSonic.Repository;
using TvdbLib.Data;
using SubSonic.Extensions;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class EpisodeProviderTest
    {
        [Test]
        public void BulkAddSpeedTest()
        {
            //Arrange
            int seriesId = 71663;
            int episodeCount = 500;
            var fakeEpisodes = Builder<TvdbSeries>.CreateNew().With(
                c => c.Episodes =
                    new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(episodeCount).
                        WhereAll()
                        .Have(l => l.Language = new TvdbLanguage(0, "eng", "a"))
                        .Build())
                    ).With(c => c.Id = seriesId).Build();

            var tvdbMock = new Mock<ITvDbProvider>();
            tvdbMock.Setup(c => c.GetSeries(seriesId, true)).Returns(fakeEpisodes).Verifiable();

            var kernel = new MockingKernel();
            kernel.Bind<IRepository>().ToConstant(MockLib.GetEmptyRepository(false)).InSingletonScope();
            kernel.Bind<ITvDbProvider>().ToConstant(tvdbMock.Object);
            kernel.Bind<IEpisodeProvider>().To<EpisodeProvider>().InSingletonScope();

            //Act
            var sw = Stopwatch.StartNew();
            kernel.Get<IEpisodeProvider>().RefreshEpisodeInfo(seriesId);


            //Assert
            tvdbMock.VerifyAll();
            Assert.Count(episodeCount, kernel.Get<IEpisodeProvider>().GetEpisodeBySeries(seriesId));
            Console.WriteLine("Duration: " + sw.Elapsed.ToString());
        }
    }
}
