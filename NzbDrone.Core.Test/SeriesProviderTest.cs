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
        public void get_unmapped()
        {
            //Setup
            var kernel = new MockingKernel();


            kernel.Bind<ISeriesProvider>().To<SeriesProvider>();
            kernel.Bind<IDiskProvider>().ToConstant(MockLib.GetStandardDisk(0, 0));
            kernel.Bind<IConfigProvider>().ToConstant(MockLib.StandardConfig);

            var seriesController = kernel.Get<ISeriesProvider>();

            //Act
            var unmappedFolder = seriesController.GetUnmappedFolders();

            //Assert
            Assert.AreElementsEqualIgnoringOrder(MockLib.StandardSeries, unmappedFolder.Values);
        }


    }
}
