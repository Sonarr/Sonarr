using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Retention;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Messaging.Commands
{
    [TestFixture]
    public class CommandQueueManagerFixture : CoreTest<CommandQueueManager>
    {
        [SetUp]
        public void Setup()
        {
            var id = 0;
            var commands = new List<CommandModel>();

            Mocker.GetMock<ICommandRepository>()
                  .Setup(s => s.Insert(It.IsAny<CommandModel>()))
                  .Returns<CommandModel>(c =>
                  {
                      c.Id = id + 1;
                      commands.Add(c);
                      id++;

                      return c;
                  });

            Mocker.GetMock<ICommandRepository>()
                  .Setup(s => s.Get(It.IsAny<int>()))
                  .Returns<int>(c =>
                  {
                      return commands.SingleOrDefault(e => e.Id == c);
                  });
        }

        [Test]
        public void should_queue_a_new_retention_command_for_changed_file_selections()
        {
            var first = Subject.Push(new EpisodeRetentionCommand { SeriesId = 1, Files = new List<int> { 1 }, EpisodeIds = new List<int>() }, trigger: CommandTrigger.Manual);
            var second = Subject.Push(new EpisodeRetentionCommand { SeriesId = 1, Files = new List<int> { 2 }, EpisodeIds = new List<int>() }, trigger: CommandTrigger.Manual);

            second.Id.Should().NotBe(first.Id);
            ((EpisodeRetentionCommand)second.Body).Files.Should().Equal(2);
            Mocker.GetMock<ICommandRepository>().Verify(v => v.Insert(It.IsAny<CommandModel>()), Times.Exactly(2));
        }

        [Test]
        public void should_reuse_a_retention_command_for_identical_selections()
        {
            var first = Subject.Push(new EpisodeRetentionCommand { SeriesId = 1, Files = new List<int> { 1 }, EpisodeIds = new List<int>() }, trigger: CommandTrigger.Manual);
            var second = Subject.Push(new EpisodeRetentionCommand { SeriesId = 1, Files = new List<int> { 1 }, EpisodeIds = new List<int>() }, trigger: CommandTrigger.Manual);

            second.Should().BeSameAs(first);
            Mocker.GetMock<ICommandRepository>().Verify(v => v.Insert(It.IsAny<CommandModel>()), Times.Once());
        }

        [Test]
        public void should_not_remove_commands_for_five_minutes_after_they_end()
        {
            var command = Subject.Push<RefreshMonitoredDownloadsCommand>(new RefreshMonitoredDownloadsCommand());

            // Start the command to mimic CommandQueue's behaviour
            command.StartedAt = DateTime.Now;
            command.Status = CommandStatus.Started;

            Subject.Start(command);
            Subject.Complete(command, "All done");
            Subject.CleanCommands();

            Subject.Get(command.Id).Should().NotBeNull();

            Mocker.GetMock<ICommandRepository>()
                  .Verify(v => v.Get(It.IsAny<int>()), Times.Never());
        }
    }
}
