using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Integration.Test.Client;
using Sonarr.Api.V3.Queue;
using Sonarr.Http;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class QueueFixture : IntegrationTest
    {
        private PagingResource<QueueResource> GetFirstPage()
        {
            var request = Queue.BuildRequest();
            request.AddParameter("includeUnknownSeriesItems", true);

            return Queue.Get<PagingResource<QueueResource>>(request);
        }

        private void RefreshQueue()
        {
            var command = Commands.Post(new SimpleCommandResource { Name = "RefreshMonitoredDownloads" });

            for (var i = 0; i < 30; i++)
            {
                var updatedCommand = Commands.Get(command.Id);

                if (updatedCommand.Status == CommandStatus.Completed)
                {
                    return;
                }

                Thread.Sleep(1000);
                i++;
            }
        }

        [Test]
        [Order(0)]
        public void ensure_queue_is_empty_when_download_client_is_configured()
        {
            EnsureNoDownloadClient();
            EnsureDownloadClient();

            var queue = GetFirstPage();

            queue.TotalRecords.Should().Be(0);
            queue.Records.Should().BeEmpty();
        }

        [Test]
        [Order(1)]
        public void ensure_queue_is_not_empty()
        {
            EnsureNoDownloadClient();

            var client = EnsureDownloadClient();
            var directory = client.Fields.First(v => v.Name == "watchFolder").Value as string;

            File.WriteAllText(Path.Combine(directory, "Series.Title.S01E01.mkv"), "Test Download");
            RefreshQueue();

            var queue = GetFirstPage();

            queue.TotalRecords.Should().Be(1);
            queue.Records.Should().NotBeEmpty();
        }
    }
}
