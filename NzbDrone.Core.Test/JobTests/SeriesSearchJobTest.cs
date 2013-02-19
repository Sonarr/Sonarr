using System.Collections.Generic;

using Moq;
using NUnit.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.JobTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class SeriesSearchJobTest : CoreTest
    {
        [Test]
        public void SeriesSearch_success()
        {
            var seasons = new List<int> { 1, 2, 3, 4, 5 };

            WithStrictMocker();

            var notification = new ProgressNotification("Series Search");

            Mocker.GetMock<SeasonProvider>()
                .Setup(c => c.GetSeasons(1)).Returns(seasons);

            Mocker.GetMock<SeasonProvider>()
                .Setup(c => c.IsIgnored(It.IsAny<int>(), It.IsAny<int>())).Returns(false);

            Mocker.GetMock<SeasonSearchJob>()
                .Setup(c => c.Start(notification, It.Is<object>(d => d.GetPropertyValue<int>("SeriesId") == 1 && d.GetPropertyValue<int>("SeasonNumber") >= 0))).Verifiable();

            //Act
            Mocker.Resolve<SeriesSearchJob>().Start(notification, new { SeriesId = 1 });

            //Assert
            Mocker.VerifyAllMocks();
            Mocker.GetMock<SeasonSearchJob>().Verify(c => c.Start(notification, It.Is<object>(d => d.GetPropertyValue<int>("SeriesId") == 1 && d.GetPropertyValue<int>("SeasonNumber") >= 0)),
                                                       Times.Exactly(seasons.Count));
        }

        [Test]
        public void SeriesSearch_no_seasons()
        {
            var seasons = new List<int>();

            WithStrictMocker();

            var notification = new ProgressNotification("Series Search");

            Mocker.GetMock<SeasonProvider>()
                .Setup(c => c.GetSeasons(1)).Returns(seasons);

            //Act
            Mocker.Resolve<SeriesSearchJob>().Start(notification, new { SeriesId = 1 });

            //Assert
            Mocker.VerifyAllMocks();
            Mocker.GetMock<SeasonSearchJob>().Verify(c => c.Start(notification, new { SeriesId = 1, SeasonNumber = It.IsAny<int>() }),
                                                       Times.Never());
        }

        [Test]
        public void SeriesSearch_should_not_search_for_season_0()
        {
            Mocker.GetMock<SeasonProvider>()
                .Setup(c => c.GetSeasons(It.IsAny<int>()))
                .Returns(new List<int> { 0, 1, 2 });

            Mocker.Resolve<SeriesSearchJob>().Start(MockNotification, new { SeriesId = 12 });


            Mocker.GetMock<SeasonSearchJob>()
                .Verify(c => c.Start(It.IsAny<ProgressNotification>(), new { SeriesId = It.IsAny<int>(), SeasonNumber = 0 }), Times.Never());
        }
    }
}