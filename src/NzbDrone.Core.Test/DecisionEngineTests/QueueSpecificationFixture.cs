using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Queue;
using NzbDrone.Core.Test.CustomFormats;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class QueueSpecificationFixture : CoreTest<QueueSpecification>
    {
        private Series _series;
        private Episode _episode;
        private RemoteEpisode _remoteEpisode;

        private Series _otherSeries;
        private Episode _otherEpisode;

        private ReleaseInfo _releaseInfo;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();

            CustomFormatsFixture.GivenCustomFormats();

            _series = Builder<Series>.CreateNew()
                                     .With(e => e.QualityProfile = new QualityProfile
                                                                {
                                                                    UpgradeAllowed = true,
                                                                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                                                                    FormatItems = CustomFormatsFixture.GetSampleFormatItems(),
                                                                    MinFormatScore = 0
                                                                })
                                     .Build();

            _episode = Builder<Episode>.CreateNew()
                                       .With(e => e.SeriesId = _series.Id)
                                       .Build();

            _otherSeries = Builder<Series>.CreateNew()
                                          .With(s => s.Id = 2)
                                          .Build();

            _otherEpisode = Builder<Episode>.CreateNew()
                                            .With(e => e.SeriesId = _otherSeries.Id)
                                            .With(e => e.Id = 2)
                                            .With(e => e.SeasonNumber = 2)
                                            .With(e => e.EpisodeNumber = 2)
                                            .Build();

            _releaseInfo = Builder<ReleaseInfo>.CreateNew()
                                               .Build();

            _remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                   .With(r => r.Series = _series)
                                                   .With(r => r.Episodes = new List<Episode> { _episode })
                                                   .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.DVD), Languages = new List<Language> { Language.Spanish } })
                                                   .With(r => r.CustomFormats = new List<CustomFormat>())
                                                   .Build();

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(x => x.ParseCustomFormat(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<Series>()))
                  .Returns(new List<CustomFormat>());
        }

        private void GivenEmptyQueue()
        {
            Mocker.GetMock<IQueueService>()
                .Setup(s => s.GetQueue())
                .Returns(new List<Queue.Queue>());
        }

        private void GivenQueueFormats(List<CustomFormat> formats)
        {
            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(x => x.ParseCustomFormat(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<Series>()))
                  .Returns(formats);
        }

        private void GivenQueue(IEnumerable<RemoteEpisode> remoteEpisodes, TrackedDownloadState trackedDownloadState = TrackedDownloadState.Downloading)
        {
            var queue = remoteEpisodes.Select(remoteEpisode => new Queue.Queue
            {
                RemoteEpisode = remoteEpisode,
                TrackedDownloadState = trackedDownloadState
            });

            Mocker.GetMock<IQueueService>()
                .Setup(s => s.GetQueue())
                .Returns(queue.ToList());
        }

        [Test]
        public void should_return_true_when_queue_is_empty()
        {
            GivenEmptyQueue();
            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_series_doesnt_match()
        {
            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Series = _otherSeries)
                                                      .With(r => r.Episodes = new List<Episode> { _episode })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .With(r => r.CustomFormats = new List<CustomFormat>())
                                                      .Build();

            GivenQueue(new List<RemoteEpisode> { remoteEpisode });
            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_everything_is_the_same()
        {
            _series.QualityProfile.Value.Cutoff = Quality.Bluray1080p.Id;

            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                .With(r => r.Series = _series)
                .With(r => r.Episodes = new List<Episode> { _episode })
                .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo
                {
                    Quality = new QualityModel(Quality.DVD),
                    Languages = new List<Language> { Language.Spanish }
                })
                .With(r => r.CustomFormats = new List<CustomFormat>())
                .With(r => r.Release = _releaseInfo)
                .Build();

            GivenQueue(new List<RemoteEpisode> { remoteEpisode });

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_quality_in_queue_is_lower()
        {
            _series.QualityProfile.Value.Cutoff = Quality.Bluray1080p.Id;

            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Series = _series)
                                                      .With(r => r.Episodes = new List<Episode> { _episode })
                                                      .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo
                                                                                       {
                                                                                           Quality = new QualityModel(Quality.SDTV),
                                                                                           Languages = new List<Language> { Language.Spanish }
                                                                                       })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .With(r => r.CustomFormats = new List<CustomFormat>())
                                                      .Build();

            GivenQueue(new List<RemoteEpisode> { remoteEpisode });
            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_quality_in_queue_is_lower_but_language_is_higher()
        {
            _series.QualityProfile.Value.Cutoff = Quality.Bluray1080p.Id;

            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Series = _series)
                                                      .With(r => r.Episodes = new List<Episode> { _episode })
                                                      .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo
                                                      {
                                                          Quality = new QualityModel(Quality.SDTV),
                                                          Languages = new List<Language> { Language.English }
                                                      })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .With(r => r.CustomFormats = new List<CustomFormat>())
                                                      .Build();

            GivenQueue(new List<RemoteEpisode> { remoteEpisode });
            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_episode_doesnt_match()
        {
            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Series = _series)
                                                      .With(r => r.Episodes = new List<Episode> { _otherEpisode })
                                                      .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo
                                                                                       {
                                                                                           Quality = new QualityModel(Quality.DVD)
                                                                                       })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .With(r => r.CustomFormats = new List<CustomFormat>())
                                                      .Build();

            GivenQueue(new List<RemoteEpisode> { remoteEpisode });
            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_qualities_are_the_same_and_languages_are_the_same_with_higher_custom_format_score()
        {
            _remoteEpisode.CustomFormats = new List<CustomFormat> { new CustomFormat("My Format", new ResolutionSpecification { Value = (int)Resolution.R1080p }) { Id = 1 } };

            var lowFormat = new List<CustomFormat> { new CustomFormat("Bad Format", new ResolutionSpecification { Value = (int)Resolution.R1080p }) { Id = 2 } };

            CustomFormatsFixture.GivenCustomFormats(_remoteEpisode.CustomFormats.First(), lowFormat.First());

            _series.QualityProfile.Value.FormatItems = CustomFormatsFixture.GetSampleFormatItems("My Format");

            GivenQueueFormats(lowFormat);

            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                .With(r => r.Series = _series)
                .With(r => r.Episodes = new List<Episode> { _episode })
                .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo
                {
                    Quality = new QualityModel(Quality.DVD),
                    Languages = new List<Language> { Language.Spanish },
                })
                .With(r => r.Release = _releaseInfo)
                .With(r => r.CustomFormats = lowFormat)
                .Build();

            GivenQueue(new List<RemoteEpisode> { remoteEpisode });
            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_qualities_are_the_same_and_languages_are_the_same()
        {
            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Series = _series)
                                                      .With(r => r.Episodes = new List<Episode> { _episode })
                                                      .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo
                                                                                       {
                                                                                           Quality = new QualityModel(Quality.DVD),
                                                                                           Languages = new List<Language> { Language.Spanish },
                                                                                       })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .With(r => r.CustomFormats = new List<CustomFormat>())
                                                      .Build();

            GivenQueue(new List<RemoteEpisode> { remoteEpisode });
            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_quality_in_queue_is_better()
        {
            _series.QualityProfile.Value.Cutoff = Quality.Bluray1080p.Id;

            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Series = _series)
                                                      .With(r => r.Episodes = new List<Episode> { _episode })
                                                      .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo
                                                                                       {
                                                                                           Quality = new QualityModel(Quality.HDTV720p),
                                                                                           Languages = new List<Language> { Language.English }
                                                                                       })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .With(r => r.CustomFormats = new List<CustomFormat>())
                                                      .Build();

            GivenQueue(new List<RemoteEpisode> { remoteEpisode });
            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_matching_multi_episode_is_in_queue()
        {
            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Series = _series)
                                                      .With(r => r.Episodes = new List<Episode> { _episode, _otherEpisode })
                                                      .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo
                                                      {
                                                          Quality = new QualityModel(Quality.HDTV720p),
                                                          Languages = new List<Language> { Language.English }
                                                      })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .With(r => r.CustomFormats = new List<CustomFormat>())
                                                      .Build();

            GivenQueue(new List<RemoteEpisode> { remoteEpisode });
            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_multi_episode_has_one_episode_in_queue()
        {
            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Series = _series)
                                                      .With(r => r.Episodes = new List<Episode> { _episode })
                                                      .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo
                                                      {
                                                          Quality = new QualityModel(Quality.HDTV720p),
                                                          Languages = new List<Language> { Language.English }
                                                      })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .With(r => r.CustomFormats = new List<CustomFormat>())
                                                      .Build();

            _remoteEpisode.Episodes.Add(_otherEpisode);

            GivenQueue(new List<RemoteEpisode> { remoteEpisode });
            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_multi_part_episode_is_already_in_queue()
        {
            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Series = _series)
                                                      .With(r => r.Episodes = new List<Episode> { _episode, _otherEpisode })
                                                      .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo
                                                      {
                                                          Quality = new QualityModel(Quality.HDTV720p),
                                                          Languages = new List<Language> { Language.English }
                                                      })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .With(r => r.CustomFormats = new List<CustomFormat>())
                                                      .Build();

            _remoteEpisode.Episodes.Add(_otherEpisode);

            GivenQueue(new List<RemoteEpisode> { remoteEpisode });
            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_multi_part_episode_has_two_episodes_in_queue()
        {
            var remoteEpisodes = Builder<RemoteEpisode>.CreateListOfSize(2)
                                                       .All()
                                                       .With(r => r.Series = _series)
                                                       .With(r => r.CustomFormats = new List<CustomFormat>())
                                                       .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo
                                                                                        {
                                                                                            Quality =
                                                                                                new QualityModel(
                                                                                                Quality.HDTV720p),
                                                                                            Languages = new List<Language> { Language.English }
                                                                                        })
                                                       .With(r => r.Release = _releaseInfo)
                                                       .TheFirst(1)
                                                       .With(r => r.Episodes = new List<Episode> { _episode })
                                                       .TheNext(1)
                                                       .With(r => r.Episodes = new List<Episode> { _otherEpisode })
                                                       .Build();

            _remoteEpisode.Episodes.Add(_otherEpisode);
            GivenQueue(remoteEpisodes);
            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_quality_and_language_in_queue_meets_cutoff()
        {
            _series.QualityProfile.Value.Cutoff = _remoteEpisode.ParsedEpisodeInfo.Quality.Quality.Id;

            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Series = _series)
                                                      .With(r => r.Episodes = new List<Episode> { _episode })
                                                      .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo
                                                      {
                                                          Quality = new QualityModel(Quality.HDTV720p),
                                                          Languages = new List<Language> { Language.Spanish }
                                                      })
                                                      .With(r => r.Release = _releaseInfo)
                                                      .With(r => r.CustomFormats = new List<CustomFormat>())
                                                      .Build();

            GivenQueue(new List<RemoteEpisode> { remoteEpisode });

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_quality_are_the_same_language_is_better_and_upgrade_allowed_is_false_for_language_profile()
        {
            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                .With(r => r.Series = _series)
                .With(r => r.Episodes = new List<Episode> { _episode })
                .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo
                {
                    Quality = new QualityModel(Quality.DVD),
                    Languages = new List<Language> { Language.English }
                })
                .With(r => r.Release = _releaseInfo)
                .With(r => r.CustomFormats = new List<CustomFormat>())
                .Build();

            GivenQueue(new List<RemoteEpisode> { remoteEpisode });
            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_quality_is_better_languages_are_the_same_and_upgrade_allowed_is_false_for_quality_profile()
        {
            _series.QualityProfile.Value.Cutoff = Quality.Bluray1080p.Id;
            _series.QualityProfile.Value.UpgradeAllowed = false;

            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                .With(r => r.Series = _series)
                .With(r => r.Episodes = new List<Episode> { _episode })
                .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo
                {
                    Quality = new QualityModel(Quality.Bluray1080p),
                    Languages = new List<Language> { Language.Spanish }
                })
                .With(r => r.Release = _releaseInfo)
                .With(r => r.CustomFormats = new List<CustomFormat>())
                .Build();

            GivenQueue(new List<RemoteEpisode> { remoteEpisode });
            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_everything_is_the_same_for_failed_pending()
        {
            _series.QualityProfile.Value.Cutoff = Quality.Bluray1080p.Id;

            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                .With(r => r.Series = _series)
                .With(r => r.Episodes = new List<Episode> { _episode })
                .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo
                {
                    Quality = new QualityModel(Quality.DVD),
                    Languages = new List<Language> { Language.Spanish }
                })
                .With(r => r.Release = _releaseInfo)
                .With(r => r.CustomFormats = new List<CustomFormat>())
                .Build();

            GivenQueue(new List<RemoteEpisode> { remoteEpisode }, TrackedDownloadState.FailedPending);

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }
    }
}
