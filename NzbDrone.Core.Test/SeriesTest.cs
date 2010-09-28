using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FizzWare.NBuilder;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Moq;
using Ninject;
using Ninject.Moq;
using NzbDrone.Core.Controllers;
using NzbDrone.Core.Repository;
using SubSonic.Repository;
using TvdbLib.Data;

// ReSharper disable InconsistentNaming
namespace NzbDrone.Core.Test
{
    [TestFixture]
    public class SeriesTest
    {
        [Test]
        [Description("This test will confirm that a folder will be skipped if it has been resolved to a series already assigned to another folder")]
        public void skip_same_series_diffrent_folder()
        {
            var tvDbId = 1234;

            //Arrange
            var moqData = new Mock<IRepository>();
            var moqTvdb = new Mock<ITvDbController>();

            //setup db to return a fake series
            Series fakeSeries = Builder<Series>.CreateNew()
                .With(f => f.TvdbId = tvDbId.ToString())
                .Build();

            moqData.Setup(f => f.Single<Series>(tvDbId)).
                Returns(fakeSeries);

            //setup tvdb to return the same show,
            IList<TvdbSearchResult> fakeSearchResult = Builder<TvdbSearchResult>.CreateListOfSize(4).WhereTheFirst(1).Has(f => f.Id = tvDbId).Build();
            
            moqTvdb.Setup(f => f.SearchSeries(It.IsAny<string>())).
                Returns(fakeSearchResult);

            var kernel = new MockingKernel();
            kernel.Bind<IRepository>().ToConstant(moqData.Object);
            kernel.Bind<ITvDbController>().ToConstant(moqTvdb.Object);

            var seriesController = kernel.Get<ISeriesController>();
            seriesController.a
        }
    }
}
