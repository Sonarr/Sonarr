using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.AutoTagging;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.TvTests.SeriesServiceTests
{
    [TestFixture]
    public class UpdateSingleSeriesFixture : CoreTest<SeriesService>
    {
        private Series _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                .With(s => s.Path = @"C:\Test\source".AsOsAgnostic())
                .With(s => s.PendingPath = null)
                .Build();

            Mocker.GetMock<IAutoTaggingService>()
                .Setup(s => s.GetTagChanges(It.IsAny<Series>()))
                .Returns(new AutoTaggingChanges());

            Mocker.GetMock<ISeriesRepository>()
                .Setup(s => s.Get(It.IsAny<int>()))
                .Returns(_series);

            Mocker.GetMock<ISeriesRepository>()
                .Setup(s => s.Update(It.IsAny<Series>()))
                .Returns<Series>(r => r);
        }

        [Test]
        public void should_defer_path_and_queue_move_when_no_move_pending()
        {
            var destinationPath = @"C:\Test\destination".AsOsAgnostic();

            var result = Subject.UpdateSeries(_series, _series.Path, destinationPath, true, out var queueMove);

            queueMove.Should().BeTrue();
            result.Path.Should().Be(@"C:\Test\source".AsOsAgnostic());
            result.PendingPath.Should().Be(destinationPath);
        }

        [Test]
        public void should_not_queue_second_move_when_one_already_pending()
        {
            var existingPendingPath = @"C:\Test\pending".AsOsAgnostic();
            _series.PendingPath = existingPendingPath;

            var result = Subject.UpdateSeries(_series, _series.Path, @"C:\Test\new-destination".AsOsAgnostic(), true, out var queueMove);

            queueMove.Should().BeFalse();
            result.Path.Should().Be(@"C:\Test\source".AsOsAgnostic());
            result.PendingPath.Should().Be(existingPendingPath);
        }

        [Test]
        public void should_update_path_immediately_when_not_moving_files()
        {
            var destinationPath = @"C:\Test\destination".AsOsAgnostic();

            var result = Subject.UpdateSeries(_series, _series.Path, destinationPath, false, out var queueMove);

            queueMove.Should().BeFalse();
            result.Path.Should().Be(destinationPath);
        }

        [Test]
        public void should_not_clear_pending_path_when_updating_immediately()
        {
            var existingPendingPath = @"C:\Test\pending".AsOsAgnostic();
            _series.PendingPath = existingPendingPath;

            var result = Subject.UpdateSeries(_series, _series.Path, @"C:\Test\destination".AsOsAgnostic(), false, out var queueMove);

            queueMove.Should().BeFalse();
            result.PendingPath.Should().Be(existingPendingPath);
        }
    }
}
