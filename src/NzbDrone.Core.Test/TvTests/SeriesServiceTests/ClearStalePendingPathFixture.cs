using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Commands;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.TvTests.SeriesServiceTests
{
    [TestFixture]
    public class ClearStalePendingPathFixture : CoreTest<SeriesService>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IManageCommandQueue>()
                .Setup(s => s.All())
                .Returns(new List<CommandModel>());
        }

        [Test]
        public void should_clear_stale_pending_path_with_no_backing_command()
        {
            var staleSeries = Builder<Series>.CreateNew()
                .With(s => s.PendingPath = @"C:\Test\destination".AsOsAgnostic())
                .Build();

            Mocker.GetMock<ISeriesRepository>()
                .Setup(s => s.All())
                .Returns(new List<Series> { staleSeries });

            Subject.Handle(new ApplicationStartedEvent());

            staleSeries.PendingPath.Should().BeNull();

            ExceptionVerification.ExpectedWarns(1);

            Mocker.GetMock<ISeriesRepository>()
                .Verify(v => v.SetFields(It.Is<IList<Series>>(l => l.Contains(staleSeries)), It.IsAny<Expression<Func<Series, object>>>()), Times.Once());
        }

        [Test]
        public void should_not_clear_pending_path_with_a_genuine_pending_move()
        {
            var genuineSeries = Builder<Series>.CreateNew()
                .With(s => s.PendingPath = @"C:\Test\destination".AsOsAgnostic())
                .Build();

            Mocker.GetMock<ISeriesRepository>()
                .Setup(s => s.All())
                .Returns(new List<Series> { genuineSeries });

            Mocker.GetMock<IManageCommandQueue>()
                .Setup(s => s.All())
                .Returns(new List<CommandModel>
                {
                    new CommandModel
                    {
                        Status = CommandStatus.Started,
                        Body = new MoveSeriesCommand { SeriesId = genuineSeries.Id }
                    }
                });

            Subject.Handle(new ApplicationStartedEvent());

            genuineSeries.PendingPath.Should().Be(@"C:\Test\destination".AsOsAgnostic());

            Mocker.GetMock<ISeriesRepository>()
                .Verify(v => v.SetFields(It.IsAny<IList<Series>>(), It.IsAny<Expression<Func<Series, object>>>()), Times.Never());
        }
    }
}
