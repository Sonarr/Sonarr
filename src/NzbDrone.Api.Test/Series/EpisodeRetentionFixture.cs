using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Retention;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Tv;
using Sonarr.Api.V5.Series;
using BadRequestException = Sonarr.Http.REST.BadRequestException;

namespace NzbDrone.Api.Test.Series
{
    [TestFixture]
    public class EpisodeRetentionFixture
    {
        private Mock<IEpisodeRetentionService> _retention;
        private Mock<IManageCommandQueue> _queue;
        private EpisodeRetentionController _controller;
        private EpisodeRetentionPlan _plan;

        [SetUp]
        public void Setup()
        {
            _retention = new Mock<IEpisodeRetentionService>();
            _queue = new Mock<IManageCommandQueue>();
            _plan = new EpisodeRetentionPlan { SeriesId = 1, Enabled = true, PreviewToken = "reviewed" };
            _retention.Setup(s => s.Preview(1)).Returns(_plan);
            _controller = new EpisodeRetentionController(_retention.Object, _queue.Object);
        }

        [Test]
        public void should_require_preview_before_queuing_cleanup()
        {
            Assert.Throws<BadRequestException>(() => _controller.Trim(1, new EpisodeRetentionRequest()));
            _queue.VerifyNoOtherCalls();
        }

        [TestCase(false, "reviewed")]
        [TestCase(true, "stale")]
        public void should_reject_disabled_or_stale_preview(bool enabled, string token)
        {
            _plan.Enabled = enabled;
            var error = Assert.Throws<NzbDroneClientException>(() => _controller.Trim(1, new EpisodeRetentionRequest { PreviewToken = token }));

            Assert.That(error.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            _queue.VerifyNoOtherCalls();
        }

        [Test]
        public void should_queue_reviewed_cleanup_as_manual_disk_command()
        {
            _queue.Setup(q => q.Push(It.IsAny<EpisodeRetentionCommand>(), CommandPriority.Normal, CommandTrigger.Manual))
                .Returns(new CommandModel { Id = 17 });

            var result = _controller.Trim(1, new EpisodeRetentionRequest { PreviewToken = "reviewed" });

            Assert.That(result.Result, Is.TypeOf<Accepted<EpisodeRetentionQueuedResource>>());
            Assert.That(((Accepted<EpisodeRetentionQueuedResource>)result.Result).Value.CommandId, Is.EqualTo(17));
            _queue.Verify(q => q.Push(It.Is<EpisodeRetentionCommand>(c => c.SeriesId == 1 && c.PreviewToken == "reviewed" && c.RequiresDiskAccess), CommandPriority.Normal, CommandTrigger.Manual), Times.Once());
            _retention.Verify(s => s.Trim(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_preview_shared_file_episodes_and_unmonitor_changes_without_trimming()
        {
            _plan.FilesToDelete.Add(new EpisodeFile { Id = 9, RelativePath = "double.mkv" });
            _plan.Episodes = new List<Episode>
            {
                new Episode { Id = 1, EpisodeFileId = 9, SeasonNumber = 1, EpisodeNumber = 1 },
                new Episode { Id = 2, EpisodeFileId = 9, SeasonNumber = 1, EpisodeNumber = 2 },
                new Episode { Id = 3, EpisodeFileId = 0, SeasonNumber = 1, EpisodeNumber = 3 }
            };
            _plan.EpisodesToUnmonitor.AddRange(_plan.Episodes);

            var result = (Ok<EpisodeRetentionResource>)_controller.Preview(1).Result;

            Assert.That(result.Value.Files[0].Episodes.Count, Is.EqualTo(2));
            Assert.That(result.Value.UnmonitorEpisodes.Count, Is.EqualTo(3));
            Assert.That(result.Value.PreviewToken, Is.EqualTo("reviewed"));
            _retention.Verify(s => s.Trim(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        }

        [TestCase(false)]
        [TestCase(true)]
        public void should_preserve_retention_when_series_resources_are_saved(bool v5)
        {
            var original = new NzbDrone.Core.Tv.Series
            {
                RetentionEnabled = true,
                RetentionKeepEpisodes = 7,
                RetentionIncludeSpecials = true
            };
            var target = new NzbDrone.Core.Tv.Series();

            if (v5)
            {
                var resource = SeriesResourceMapper.ToResource(original);
                SeriesResourceMapper.ToModel(resource, target);
            }
            else
            {
                var resource = Sonarr.Api.V3.Series.SeriesResourceMapper.ToResource(original);
                Sonarr.Api.V3.Series.SeriesResourceMapper.ToModel(resource, target);
            }

            Assert.That(target.RetentionEnabled, Is.True);
            Assert.That(target.RetentionKeepEpisodes, Is.EqualTo(7));
            Assert.That(target.RetentionIncludeSpecials, Is.True);
        }
    }
}
