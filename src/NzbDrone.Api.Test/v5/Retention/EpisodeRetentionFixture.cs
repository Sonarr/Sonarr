using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Retention;
using NzbDrone.Core.Tv;
using Sonarr.Api.V5.Series;

namespace NzbDrone.Api.Test.v5.Retention
{
    [TestFixture]
    public class EpisodeRetentionFixture
    {
        private Mock<IEpisodeRetentionService> _retention;
        private EpisodeRetentionController _controller;
        private EpisodeRetentionPlan _plan;

        [SetUp]
        public void Setup()
        {
            _retention = new Mock<IEpisodeRetentionService>();
            _plan = new EpisodeRetentionPlan { SeriesId = 1, Enabled = true };
            _retention.Setup(s => s.Preview(1)).Returns(_plan);
            _controller = new EpisodeRetentionController(_retention.Object);
        }

        [Test]
        public void should_preview_shared_file_episodes_and_unmonitor_changes()
        {
            _plan.FilesToDelete.Add(new EpisodeFile { Id = 9, RelativePath = "double.mkv" });
            _plan.Episodes = new List<Episode>
            {
                new Episode { Id = 1, EpisodeFileId = 9, SeasonNumber = 1, EpisodeNumber = 1 },
                new Episode { Id = 2, EpisodeFileId = 9, SeasonNumber = 1, EpisodeNumber = 2 },
                new Episode { Id = 3, EpisodeFileId = 0, SeasonNumber = 1, EpisodeNumber = 3 }
            };
            _plan.EpisodesToUnmonitor.AddRange(_plan.Episodes);
            _plan.OlderMissingEpisodesToUnmonitor.Add(_plan.Episodes[2]);

            var result = (Ok<EpisodeRetentionResource>)_controller.Preview(1).Result;

            Assert.That(result.Value.Files[0].Id, Is.EqualTo(9));
            Assert.That(result.Value.Files[0].RelativePath, Is.EqualTo("double.mkv"));
            Assert.That(result.Value.Files[0].Episodes.Select(e => e.Id), Is.EqualTo(new[] { 1, 2 }));
            Assert.That(result.Value.UnmonitorEpisodes.Select(e => e.Id), Is.EqualTo(new[] { 3 }));
            _retention.Verify(s => s.Preview(1), Times.Once());
            _retention.VerifyNoOtherCalls();
        }

        [Test]
        public void should_return_not_found_for_unknown_series()
        {
            _retention.Setup(s => s.Preview(2))
                .Throws(new ModelNotFoundException(typeof(NzbDrone.Core.Tv.Series), 2));

            Assert.That(_controller.Preview(2).Result, Is.TypeOf<NotFound>());
        }

        [Test]
        public void should_return_an_empty_preview_when_no_changes_are_needed()
        {
            var result = (Ok<EpisodeRetentionResource>)_controller.Preview(1).Result;

            Assert.That(result.Value.Files, Is.Empty);
            Assert.That(result.Value.UnmonitorEpisodes, Is.Empty);
        }

        [Test]
        public void should_round_trip_retention_through_v5()
        {
            var original = new NzbDrone.Core.Tv.Series
            {
                RetentionEnabled = true,
                RetentionKeepEpisodes = 7,
                RetentionIncludeSpecials = true
            };
            var target = new NzbDrone.Core.Tv.Series();

            var resource = SeriesResourceMapper.ToResource(original);
            SeriesResourceMapper.ToModel(resource, target);

            Assert.That(target.RetentionEnabled, Is.True);
            Assert.That(target.RetentionKeepEpisodes, Is.EqualTo(7));
            Assert.That(target.RetentionIncludeSpecials, Is.True);
        }

        [TestCase(true, 7, true)]
        [TestCase(false, 12, false)]
        public void should_preserve_existing_retention_when_v3_updates_other_settings(bool enabled, int keepEpisodes, bool includeSpecials)
        {
            var original = new NzbDrone.Core.Tv.Series
            {
                RetentionEnabled = enabled,
                RetentionKeepEpisodes = keepEpisodes,
                RetentionIncludeSpecials = includeSpecials,
                Monitored = true,
                QualityProfileId = 1
            };
            var resource = Sonarr.Api.V3.Series.SeriesResourceMapper.ToResource(original);
            resource.Monitored = false;
            resource.QualityProfileId = 2;

            var updated = Sonarr.Api.V3.Series.SeriesResourceMapper.ToModel(resource, original);

            Assert.That(updated, Is.SameAs(original));
            Assert.That(updated.Monitored, Is.False);
            Assert.That(updated.QualityProfileId, Is.EqualTo(2));
            Assert.That(updated.RetentionEnabled, Is.EqualTo(enabled));
            Assert.That(updated.RetentionKeepEpisodes, Is.EqualTo(keepEpisodes));
            Assert.That(updated.RetentionIncludeSpecials, Is.EqualTo(includeSpecials));
        }
    }
}
