using System.Collections.Generic;
using FizzWare.NBuilder;
using NUnit.Framework;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class RemovedSeriesCheckFixture : CoreTest<RemovedSeriesCheck>
    {
        private void GivenSeries(int amount, int deleted)
        {
            List<Series> series;

            if (amount == 0)
            {
                series = new List<Series>();
            }
            else if (deleted == 0)
            {
                series = Builder<Series>.CreateListOfSize(amount)
                    .All()
                    .With(v => v.Status = SeriesStatusType.Continuing)
                    .BuildList();
            }
            else
            {
                series = Builder<Series>.CreateListOfSize(amount)
                    .All()
                    .With(v => v.Status = SeriesStatusType.Continuing)
                    .Random(deleted)
                    .With(v => v.Status = SeriesStatusType.Deleted)
                    .BuildList();
            }

            Mocker.GetMock<ISeriesService>()
                .Setup(v => v.GetAllSeries())
                .Returns(series);
        }

        [Test]
        public void should_return_error_if_series_no_longer_on_tvdb()
        {
            GivenSeries(4, 1);

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_return_error_if_multiple_series_no_longer_on_tvdb()
        {
            GivenSeries(4, 2);

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_return_ok_if_all_series_still_on_tvdb()
        {
            GivenSeries(4, 0);

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_ok_if_no_series_exist()
        {
            GivenSeries(0, 0);

            Subject.Check().ShouldBeOk();
        }
    }
}
