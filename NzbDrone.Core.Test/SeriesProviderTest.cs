using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FizzWare.NBuilder;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Moq;
using Ninject;
using Ninject.Moq;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using SubSonic.Repository;
using TvdbLib.Data;
using System.Linq;

// ReSharper disable InconsistentNaming
namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class SeriesProviderTest
    {
        [Test]
        public void Map_path_to_series()
        {
            //Arrange
            TvdbSeries fakeSeries = Builder<TvdbSeries>.CreateNew().With(f => f.SeriesName = "The Simpsons").Build();
            var fakeSearch = Builder<TvdbSearchResult>.CreateNew().Build();
            fakeSearch.Id = fakeSeries.Id;
            fakeSearch.SeriesName = fakeSeries.SeriesName;

            var moqData = new Mock<IRepository>();
            var moqTvdb = new Mock<ITvDbProvider>();

            moqData.Setup(f => f.Exists<Series>(c => c.SeriesId == It.IsAny<int>())).Returns(false);

            moqTvdb.Setup(f => f.GetSeries(It.IsAny<String>())).Returns(fakeSearch);
            moqTvdb.Setup(f => f.GetSeries(fakeSeries.Id, false)).Returns(fakeSeries).Verifiable();

            var kernel = new MockingKernel();
            kernel.Bind<IRepository>().ToConstant(moqData.Object);
            kernel.Bind<ITvDbProvider>().ToConstant(moqTvdb.Object);
            kernel.Bind<ISeriesProvider>().To<SeriesProvider>();

            //Act
            var seriesController = kernel.Get<ISeriesProvider>();
            var mappedSeries = seriesController.MapPathToSeries(@"D:\TV Shows\The Simpsons");

            //Assert
            moqTvdb.VerifyAll();
            Assert.AreEqual(fakeSeries, mappedSeries);
        }

        [Test]
        public void Add_new_series()
        {
            var repo = MockLib.GetEmptyRepository();

            var kernel = new MockingKernel();
            kernel.Bind<ISeriesProvider>().To<SeriesProvider>();
            kernel.Bind<IRepository>().ToConstant(repo);

            string path = "C:\\Test\\";
            int tvDbId = 1234;
            int qualityProfileId = 2;

            //Act
            var seriesProvider = kernel.Get<ISeriesProvider>();
            seriesProvider.AddSeries(path, tvDbId, qualityProfileId);


            //Assert
            var series = seriesProvider.GetAllSeries();
            Assert.IsNotEmpty(series);
            Assert.Count(1, series);
            Assert.AreEqual(path, series.First().Path);
            Assert.AreEqual(tvDbId, series.First().SeriesId);
            Assert.AreEqual(qualityProfileId, series.First().QualityProfileId);

        }



        [Test]
        [Row(new object[] { "That's Life - 2x03 -The Devil and Miss DeLucca", "That's Life" })]
        [Row(new object[] { "Van.Duin.Op.Zn.Best.S02E05.DUTCH.WS.PDTV.XViD-DiFFERENT", "Van Duin Op Zn Best" })]
        [Row(new object[] { "Dollhouse.S02E06.The.Left.Hand.720p.BluRay.x264-SiNNERS", "Dollhouse" })]
        [Row(new object[] { "Heroes.S02.COMPLETE.German.PROPER.DVDRip.XviD-Prim3time", "Heroes" })]
        [Ignore("should be updated to validate agains a remote episode instance rather than just the title string")]
        public void Test_Parse_Success(string postTitle, string title)
        {
            var result = Parser.ParseEpisodeInfo(postTitle);
            //Assert.AreEqual(title, result, postTitle);
        }

        [Test]
        public void Test_is_monitored()
        {
            var kernel = new MockingKernel();
            var repo = MockLib.GetEmptyRepository();
            kernel.Bind<IRepository>().ToConstant(repo);
            kernel.Bind<ISeriesProvider>().To<SeriesProvider>();

            repo.Add(Builder<Series>.CreateNew()
                .With(c => c.Monitored = true)
                .With(c => c.SeriesId = 12)
                .Build());

            repo.Add(Builder<Series>.CreateNew()
            .With(c => c.Monitored = false)
            .With(c => c.SeriesId = 11)
            .Build());


            //Act, Assert
            var provider = kernel.Get<ISeriesProvider>();
            Assert.IsTrue(provider.IsMonitored(12));
            Assert.IsFalse(provider.IsMonitored(11));
            Assert.IsFalse(provider.IsMonitored(1));
        }



        [Test]
        [Row(12, QualityTypes.TV, true)]
        [Row(12, QualityTypes.Unknown, false)]
        [Row(12, QualityTypes.Bluray1080, false)]
        [Row(12, QualityTypes.Bluray720, false)]
        [Row(12, QualityTypes.HDTV, false)]
        [Row(12, QualityTypes.WEBDL, false)]
        public void QualityWanted(int seriesId, QualityTypes qualityTypes, Boolean result)
        {
            var kernel = new MockingKernel();
            var repo = MockLib.GetEmptyRepository();
            kernel.Bind<IRepository>().ToConstant(repo);
            kernel.Bind<ISeriesProvider>().To<SeriesProvider>();

            var quality = Builder<QualityProfile>.CreateNew()
                .With(q => q.Allowed = new List<QualityTypes>() { QualityTypes.BDRip, QualityTypes.DVD, QualityTypes.TV })
                .With(q => q.Cutoff = QualityTypes.DVD)
                .Build();

            var qualityProviderMock = new Mock<IQualityProvider>();
            qualityProviderMock.Setup(c => c.Find(quality.QualityProfileId)).Returns(quality).Verifiable();
            kernel.Bind<IQualityProvider>().ToConstant(qualityProviderMock.Object);


            repo.Add(Builder<Series>.CreateNew()
                .With(c => c.SeriesId = 12)
                .With(c => c.QualityProfileId = quality.QualityProfileId)
                .Build());

            //Act
            var needed = kernel.Get<ISeriesProvider>().QualityWanted(seriesId, qualityTypes);

            Assert.AreEqual(result, needed);


        }
    }
}
