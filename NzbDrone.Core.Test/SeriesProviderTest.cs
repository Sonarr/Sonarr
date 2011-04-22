using System;
using System.Collections.Generic;
using System.Linq;
using AutoMoq;
using FizzWare.NBuilder;
using MbUnit.Framework;
using Moq;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using SubSonic.Repository;
using TvdbLib.Data;

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
            var fakeSeries = Builder<TvdbSeries>.CreateNew()
                .With(f => f.SeriesName = "The Simpsons")
                .Build();

            var fakeSearch = Builder<TvdbSearchResult>.CreateNew()
                .With(s => s.Id = fakeSeries.Id)
                .With(s => s.SeriesName = fakeSeries.SeriesName)
                .Build();


            var mocker = new AutoMoqer();

            mocker.GetMock<IRepository>()
                .Setup(f => f.Exists<Series>(c => c.SeriesId == It.IsAny<int>()))
                .Returns(false);

            mocker.GetMock<TvDbProvider>()
                .Setup(f => f.GetSeries(It.IsAny<String>()))
                .Returns(fakeSearch);
            mocker.GetMock<TvDbProvider>()
                .Setup(f => f.GetSeries(fakeSeries.Id, false))
                .Returns(fakeSeries)
                .Verifiable();

            //Act

            var mappedSeries = mocker.Resolve<SeriesProvider>().MapPathToSeries(@"D:\TV Shows\The Simpsons");

            //Assert
            mocker.GetMock<TvDbProvider>().VerifyAll();
            Assert.AreEqual(fakeSeries, mappedSeries);
        }

        [Test]
        public void Add_new_series()
        {
            var mocker = new AutoMoqer();
            mocker.SetConstant(MockLib.GetEmptyRepository());

            string path = "C:\\Test\\";
            int tvDbId = 1234;
            int qualityProfileId = 2;

            //Act
            var seriesProvider = mocker.Resolve<SeriesProvider>();
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
        public void find_series_empty_repo()
        {
            var mocker = new AutoMoqer();
            mocker.SetConstant(MockLib.GetEmptyRepository());

            //Act
            var seriesProvider = mocker.Resolve<SeriesProvider>();
            var series = seriesProvider.FindSeries("My Title");


            //Assert
            Assert.IsNull(series);
        }

        [Test]
        public void find_series_empty_match()
        {
            var mocker = new AutoMoqer();
            var emptyRepository = MockLib.GetEmptyRepository();
            mocker.SetConstant(emptyRepository);
            emptyRepository.Add<Series>(MockLib.GetFakeSeries(1, "MyTitle"));
            //Act
            var seriesProvider = mocker.Resolve<SeriesProvider>();
            var series = seriesProvider.FindSeries("WrongTitle");


            //Assert
            Assert.IsNull(series);
        }

        [Test]
        [Row("The Test", "Test")]
        [Row("The Test Title", "test title")]
        public void find_series_match(string title, string searchTitle)
        {
            var mocker = new AutoMoqer();
            var emptyRepository = MockLib.GetEmptyRepository();
            mocker.SetConstant(emptyRepository);
            emptyRepository.Add<Series>(MockLib.GetFakeSeries(1, title));
            //Act
            var seriesProvider = mocker.Resolve<SeriesProvider>();
            var series = seriesProvider.FindSeries(searchTitle);


            //Assert
            Assert.IsNotNull(series);
            Assert.AreEqual(title, series.Title);
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
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyRepository());

            mocker.Resolve<IRepository>().Add(Builder<Series>.CreateNew()
                                                  .With(c => c.Monitored = true)
                                                  .With(c => c.SeriesId = 12)
                                                  .Build());

            mocker.Resolve<IRepository>().Add(Builder<Series>.CreateNew()
                                                  .With(c => c.Monitored = false)
                                                  .With(c => c.SeriesId = 11)
                                                  .Build());


            //Act, Assert
            var provider = mocker.Resolve<SeriesProvider>();
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
            var quality = Builder<QualityProfile>.CreateNew()
                .With(q => q.Allowed = new List<QualityTypes> { QualityTypes.BDRip, QualityTypes.DVD, QualityTypes.TV })
                .With(q => q.Cutoff = QualityTypes.DVD)
                .Build();

            var series = Builder<Series>.CreateNew()
                .With(c => c.SeriesId = 12)
                .With(c => c.QualityProfileId = quality.QualityProfileId)
                .Build();

            var mocker = new AutoMoqer();
            var emptyRepository = MockLib.GetEmptyRepository();
            mocker.SetConstant(emptyRepository);


            mocker.GetMock<QualityProvider>()
                .Setup(c => c.Find(quality.QualityProfileId)).Returns(quality);


            emptyRepository.Add(series);

            //Act
            var needed = mocker.Resolve<SeriesProvider>().QualityWanted(seriesId, qualityTypes);

            Assert.AreEqual(result, needed);
        }
    }
}