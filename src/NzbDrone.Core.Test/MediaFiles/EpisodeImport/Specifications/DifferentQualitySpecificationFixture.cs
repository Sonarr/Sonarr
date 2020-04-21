using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Marr.Data;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class DifferentQualitySpecificationFixture : CoreTest<DifferentQualitySpecification>
    {
        private LocalEpisode _localEpisode;
        private DownloadClientItem _downloadClientItem;

        [SetUp]
        public void Setup()
        {
            var qualityProfile = new QualityProfile
            {
                Cutoff = Quality.Bluray1080p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(Quality.DVD, Quality.HDTV720p, Quality.Bluray1080p)
            };

            var fakeSeries = Builder<Series>.CreateNew()
                                            .With(c => c.QualityProfile = qualityProfile)
                                            .Build();

            _localEpisode = Builder<LocalEpisode>.CreateNew()
                                                 .With(l => l.Quality = new QualityModel(Quality.Bluray1080p))
                                                 .With(l => l.DownloadClientEpisodeInfo = new ParsedEpisodeInfo())
                                                 .With(l => l.Series = fakeSeries)
                                                 .Build();

            _downloadClientItem = Builder<DownloadClientItem>.CreateNew()
                                                             .Build();
        }

        private void GivenGrabbedEpisodeHistory(QualityModel quality)
        {
            var history = Builder<EpisodeHistory>.CreateListOfSize(1)
                                                                 .TheFirst(1)
                                                                 .With(h => h.Quality = quality)
                                                                 .With(h => h.EventType = EpisodeHistoryEventType.Grabbed)
                                                                 .BuildList();

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(history);
        }

        [Test]
        public void should_be_accepted_if_no_download_client_item()
        {
            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_quality_does_not_match_for_full_season_pack()
        {
            GivenGrabbedEpisodeHistory(new QualityModel(Quality.SDTV));
            _localEpisode.DownloadClientEpisodeInfo.FullSeason = true;

            Subject.IsSatisfiedBy(_localEpisode, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_no_grabbed_episode_history()
        {
            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(new List<EpisodeHistory>());

            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .TheFirst(1)
                                                     .With(e => e.EpisodeFileId = 0)
                                                     .BuildList();

            Subject.IsSatisfiedBy(_localEpisode, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_quality_matches()
        {
            GivenGrabbedEpisodeHistory(_localEpisode.Quality);

            Subject.IsSatisfiedBy(_localEpisode, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_rejected_if_quality_does_not_match()
        {
            GivenGrabbedEpisodeHistory(new QualityModel(Quality.SDTV));

            Subject.IsSatisfiedBy(_localEpisode, _downloadClientItem).Accepted.Should().BeFalse();
        }
    }
}