using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class GrabbedReleaseQualityFixture : CoreTest<GrabbedReleaseQualitySpecification>
    {
        private LocalEpisode _localEpisode;
        private DownloadClientItem _downloadClientItem;

        [SetUp]
        public void Setup()
        {
            _localEpisode = Builder<LocalEpisode>.CreateNew()
                                                 .With(l => l.Quality = new QualityModel(Quality.Bluray720p))
                                                 .Build();

            _downloadClientItem = Builder<DownloadClientItem>.CreateNew()
                                                             .Build();
        }

        private void GivenHistory(List<History.History> history)
        {
            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(history);
        }

        [Test]
        public void should_be_accepted_when_downloadClientItem_is_null()
        {
            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_no_history_for_downloadId()
        {
            GivenHistory(new List<History.History>());

            Subject.IsSatisfiedBy(_localEpisode, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_no_grabbed_history_for_downloadId()
        {
            var history = Builder<History.History>.CreateListOfSize(1)
                                                  .All()
                                                  .With(h => h.EventType = HistoryEventType.Unknown)
                                                  .BuildList();

            GivenHistory(history);

            Subject.IsSatisfiedBy(_localEpisode, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_grabbed_history_quality_matches()
        {
            var history = Builder<History.History>.CreateListOfSize(1)
                                                  .All()
                                                  .With(h => h.EventType = HistoryEventType.Grabbed)
                                                  .With(h => h.Quality = _localEpisode.Quality)
                                                  .BuildList();

            GivenHistory(history);

            Subject.IsSatisfiedBy(_localEpisode, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_rejected_if_grabbed_history_quality_does_not_match()
        {
            var history = Builder<History.History>.CreateListOfSize(1)
                                                  .All()
                                                  .With(h => h.EventType = HistoryEventType.Grabbed)
                                                  .With(h => h.Quality = new QualityModel(Quality.HDTV720p))
                                                  .BuildList();

            GivenHistory(history);

            Subject.IsSatisfiedBy(_localEpisode, _downloadClientItem).Accepted.Should().BeFalse();
        }
    }
}