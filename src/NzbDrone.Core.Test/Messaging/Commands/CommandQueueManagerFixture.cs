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
        private const string PreviewToken = "4792CEB9496AADF3E3E849D2E01EF8F10517635CBD38FE3D3E2BAF129682A134";
        private const string ChangedPreviewToken = "35D38B9644844E321300DC5B88DCA0BCCA2B31D903A34B7C7641554B7B363B5F";

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
        public void should_queue_a_new_retention_command_for_a_changed_preview_token()
        {
            var first = Subject.Push(new EpisodeRetentionCommand { SeriesId = 1, PreviewToken = PreviewToken }, trigger: CommandTrigger.Manual);
            var second = Subject.Push(new EpisodeRetentionCommand { SeriesId = 1, PreviewToken = ChangedPreviewToken }, trigger: CommandTrigger.Manual);

            second.Id.Should().NotBe(first.Id);
            ((EpisodeRetentionCommand)second.Body).PreviewToken.Should().Be(ChangedPreviewToken);
            Mocker.GetMock<ICommandRepository>().Verify(v => v.Insert(It.IsAny<CommandModel>()), Times.Exactly(2));
        }

        [Test]
        public void should_reuse_a_retention_command_for_an_identical_preview_token()
        {
            var first = Subject.Push(new EpisodeRetentionCommand { SeriesId = 1, PreviewToken = PreviewToken }, trigger: CommandTrigger.Manual);
            var second = Subject.Push(new EpisodeRetentionCommand { SeriesId = 1, PreviewToken = PreviewToken }, trigger: CommandTrigger.Manual);

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
