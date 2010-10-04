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
    public class SeriesTest
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

            moqData.Setup(f => f.Exists<Series>(c => c.TvdbId == It.IsAny<long>())).Returns(false);

            moqTvdb.Setup(f => f.GetSeries(It.IsAny<String>())).Returns(fakeSearch);
            moqTvdb.Setup(f => f.GetSeries(fakeSeries.Id, It.IsAny<TvdbLanguage>())).Returns(fakeSeries);

            var kernel = new MockingKernel();
            kernel.Bind<IRepository>().ToConstant(moqData.Object);
            kernel.Bind<ITvDbProvider>().ToConstant(moqTvdb.Object);
            kernel.Bind<ISeriesProvider>().To<SeriesProvider>();

            //Act
            var seriesController = kernel.Get<ISeriesProvider>();
            var mappedSeries = seriesController.MapPathToSeries(@"D:\TV Shows\The Simpsons");

            //Assert
            Assert.AreEqual(fakeSeries, mappedSeries);
        }

        [Test]
        [Row(new object[] { "That's Life - 2x03 -The Devil and Miss DeLucca", "That's Life" })]
        [Row(new object[] { "Van.Duin.Op.Zn.Best.S02E05.DUTCH.WS.PDTV.XViD-DiFFERENT", "Van Duin Op Zn Best" })]
        [Row(new object[] { "Dollhouse.S02E06.The.Left.Hand.720p.BluRay.x264-SiNNERS", "Dollhouse" })]
        [Row(new object[] { "Heroes.S02.COMPLETE.German.PROPER.DVDRip.XviD-Prim3time", "Heroes" })]
        public void Test_Parse_Success(string postTitle, string title)
        {
            var result = SeriesProvider.ParseTitle(postTitle);
            Assert.AreEqual(title, result, postTitle);
        }



        [Test]
        [Description("This test confirms that the tvdb id stored in the db is preserved rather than being replaced by an auto incrementing value")]
        public void tvdbid_is_preserved([RandomNumbers(Minimum = 100, Maximum = 999, Count = 1)] int tvdbId)
        {
            //Arrange
            var sonicRepo = MockLib.GetEmptyRepository();
            var series = Builder<Series>.CreateNew().With(c => c.TvdbId = tvdbId).Build();

            //Act
            var addId = sonicRepo.Add(series);

            //Assert
            Assert.AreEqual(tvdbId, addId);
            var allSeries = sonicRepo.All<Series>();
            Assert.IsNotEmpty(allSeries);
            Assert.AreEqual(tvdbId, allSeries.First().TvdbId);
        }

        [Test]
        [Row(new object[] { "CAPITAL", "capital", true })]
        [Row(new object[] { "Something!!", "Something", true })]
        [Row(new object[] { "Simpsons 2000", "Simpsons", true })]
        [Row(new object[] { "Simp222sons", "Simpsons", true })]
        [Row(new object[] { "Simpsons", "The Simpsons", true })]
        [Row(new object[] { "Law and order", "Law & order", true })]
        [Row(new object[] { "xxAndxx", "xxxx", false })]
        [Row(new object[] { "Andxx", "xx", false })]
        [Row(new object[] { "xxAnd", "xx", false })]
        [Row(new object[] { "Thexx", "xx", false })]
        [Row(new object[] { "Thexx", "xx", false })]
        [Row(new object[] { "xxThexx", "xxxxx", false })]
        [Row(new object[] { "Simpsons The", "Simpsons", true })]
        public void Name_match_test(string a, string b, bool match)
        {
            bool result = TvDbProvider.IsTitleMatch(a, b);

            Assert.AreEqual(match, result, "{0} , {1}", a, b);
        }
    }
}
