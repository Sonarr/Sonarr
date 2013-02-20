using System;
using System.Linq;
using System.Net;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class ReferenceDataProviderTest : SqlCeTest
    {
        private const string validSeriesIds = "[1,2,3,4,5]";
        private const string invalidSeriesIds = "[1,2,NaN,4,5]";

        private const string url = "http://services.nzbdrone.com/DailySeries/AllIds";

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ConfigProvider>().SetupGet(s => s.ServiceRootUrl)
                    .Returns("http://services.nzbdrone.com");
        }


        [Test]
        public void GetDailySeriesIds_should_return_list_of_int_when_all_are_valid()
        {
            //Setup
            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadString(url))
                    .Returns(validSeriesIds);

            //Act
            var result = Mocker.Resolve<ReferenceDataProvider>().GetDailySeriesIds();

            //Assert
            result.Should().HaveCount(5);
        }

        [Test]
        public void GetDailySeriesIds_should_return_empty_list_when_unable_to_parse()
        {
            //Setup
            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadString(url))
                    .Returns(invalidSeriesIds);

            //Act
            var result = Mocker.Resolve<ReferenceDataProvider>().GetDailySeriesIds();

            //Assert
            result.Should().BeEmpty();
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void GetDailySeriesIds_should_return_empty_list_of_int_when_server_is_unavailable()
        {
            //Setup
            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadString(url))
                    .Throws(new Exception());

            //Act
            var result = Mocker.Resolve<ReferenceDataProvider>().GetDailySeriesIds();

            //Assert
            result.Should().HaveCount(0);
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void IsDailySeries_should_return_true()
        {
            //Setup
            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadString(url))
                    .Returns(validSeriesIds);

            //Act
            var result = Mocker.Resolve<ReferenceDataProvider>().IsSeriesDaily(1);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsDailySeries_should_return_false()
        {
            //Setup
            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadString(url))
                    .Returns(validSeriesIds);

            //Act
            var result = Mocker.Resolve<ReferenceDataProvider>().IsSeriesDaily(10);

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void UpdateDailySeries_should_update_series_that_match_daily_series_list()
        {
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateListOfSize(5)
                .All()
                .With(s => s.SeriesType = SeriesType.Standard)
                .Build();

            Db.InsertMany(fakeSeries);

            //Setup
            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadString(url))
                    .Returns(validSeriesIds);

            //Act
            Mocker.Resolve<ReferenceDataProvider>().UpdateDailySeries();

            //Assert
            var result = Db.Fetch<Series>();

            result.Where(s => s.SeriesType == SeriesType.Daily).Should().HaveCount(5);
        }

        [Test]
        public void UpdateDailySeries_should_update_series_should_skip_series_that_dont_match()
        {
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateListOfSize(5)
                .All()
                .With(s => s.SeriesType = SeriesType.Standard)
                .TheFirst(1)
                .With(s => s.SeriesId = 10)
                .TheNext(1)
                .With(s => s.SeriesId = 11)
                .TheNext(1)
                .With(s => s.SeriesId = 12)
                .Build();

            Db.InsertMany(fakeSeries);

            //Setup
            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadString(url))
                    .Returns(validSeriesIds);

            //Act
            Mocker.Resolve<ReferenceDataProvider>().UpdateDailySeries();

            //Assert
            var result = Db.Fetch<Series>();

            result.Where(s => s.SeriesType == SeriesType.Standard).Should().HaveCount(3);
            result.Where(s => s.SeriesType == SeriesType.Daily).Should().HaveCount(2);
        }

        [Test]
        public void UpdateDailySeries_should_update_series_should_not_overwrite_existing_isDaily()
        {
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateListOfSize(5)
                .All()
                .With(s => s.SeriesType = SeriesType.Standard)
                .TheFirst(1)
                .With(s => s.SeriesId = 10)
                .With(s => s.SeriesType = SeriesType.Daily)
                .TheNext(1)
                .With(s => s.SeriesId = 11)
                .TheNext(1)
                .With(s => s.SeriesId = 12)
                .Build();

            Db.InsertMany(fakeSeries);

            //Setup
            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadString(url))
                    .Returns(validSeriesIds);

            //Act
            Mocker.Resolve<ReferenceDataProvider>().UpdateDailySeries();

            //Assert
            var result = Db.Fetch<Series>();

            result.Where(s => s.SeriesType == SeriesType.Daily).Should().HaveCount(3);
            result.Where(s => s.SeriesType == SeriesType.Standard).Should().HaveCount(2);
        }

        [Test]
        public void broken_service_should_not_cause_this_call_to_fail()
        {
            WithRealDb();

            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadString(It.IsAny<string>()))
                .Throws(new WebException())
                .Verifiable();

            Mocker.Resolve<ReferenceDataProvider>().UpdateDailySeries();

            ExceptionVerification.ExpectedWarns(1);
            Mocker.VerifyAllMocks();
        }
    }
}