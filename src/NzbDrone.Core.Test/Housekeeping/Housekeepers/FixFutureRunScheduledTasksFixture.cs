using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Microsoft.Practices.ObjectBuilder2;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class FixFutureRunScheduledTasksFixture : DbTest<FixFutureRunScheduledTasks, ScheduledTask>
    {
        [Test]
        public void should_set_last_execution_time_to_now_when_its_in_the_future()
        {
            var tasks = Builder<ScheduledTask>.CreateListOfSize(5)
                                              .All()
                                              .With(t => t.LastExecution = DateTime.UtcNow.AddDays(5))
                                              .BuildListOfNew();

            Db.InsertMany(tasks);

            Subject.Clean();

            AllStoredModels.ForEach(t => t.LastExecution.Should().BeBefore(DateTime.UtcNow));
        }

        [Test]
        public void should_not_change_last_execution_time_when_its_in_the_past()
        {
            var expectedTime = DateTime.UtcNow.AddHours(-1);

            var tasks = Builder<ScheduledTask>.CreateListOfSize(5)
                                              .All()
                                              .With(t => t.LastExecution = expectedTime)
                                              .BuildListOfNew();

            Db.InsertMany(tasks);

            Subject.Clean();

            AllStoredModels.ForEach(t => t.LastExecution.Should().Be(expectedTime));
        }
    }
}