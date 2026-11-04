using System;
using System.Threading;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.Retention;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Tv.Commands;

namespace NzbDrone.Core.Test.Messaging.Commands
{
    [TestFixture]
    public class CommandQueueFixture
    {
        [TestCase(true)]
        [TestCase(false)]
        public void should_wait_for_running_refresh_or_scan_before_retention(bool refresh)
        {
            var queue = new CommandQueue();
            var running = new CommandModel
            {
                Body = refresh ? (Command)new RefreshSeriesCommand() : new RescanSeriesCommand(),
                Status = CommandStatus.Started
            };
            var retention = new CommandModel { Body = new EpisodeRetentionCommand(), Status = CommandStatus.Queued };
            queue.Add(running);
            queue.Add(retention);

            using (var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(100)))
            using (var blocked = queue.GetConsumingEnumerable(cancellation.Token).GetEnumerator())
            {
                Assert.That(blocked.MoveNext(), Is.False);
            }

            running.Status = CommandStatus.Completed;
            using (var ready = queue.GetConsumingEnumerable().GetEnumerator())
            {
                Assert.That(ready.MoveNext(), Is.True);
                Assert.That(ready.Current, Is.SameAs(retention));
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void should_block_refresh_or_scan_while_retention_runs(bool refresh)
        {
            var queue = new CommandQueue();
            queue.Add(new CommandModel { Body = new EpisodeRetentionCommand(), Status = CommandStatus.Started });
            queue.Add(new CommandModel
            {
                Body = refresh ? (Command)new RefreshSeriesCommand() : new RescanSeriesCommand(),
                Status = CommandStatus.Queued
            });

            using (var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(100)))
            using (var blocked = queue.GetConsumingEnumerable(cancellation.Token).GetEnumerator())
            {
                Assert.That(blocked.MoveNext(), Is.False);
            }
        }
    }
}
