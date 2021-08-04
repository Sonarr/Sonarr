using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Languages;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Profiles.Releases;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class UpgradeSpecificationFixture : CoreTest<UpgradeSpecification>
    {
        private Series _series;
        private LocalEpisode _localEpisode;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .With(s => s.SeriesType = SeriesTypes.Standard)
                                     .With(e => e.QualityProfile = new QualityProfile
                                        {
                                            Items = Qualities.QualityFixture.GetDefaultQualities(),
                                        })
                                     .With(l => l.LanguageProfile = new LanguageProfile
                                        {
                                            Languages = Languages.LanguageFixture.GetDefaultLanguages(),
                                            Cutoff = Language.Spanish,
                                        })
                                     .Build();

            _localEpisode = new LocalEpisode
                                {
                                    Path = @"C:\Test\30 Rock\30.rock.s01e01.avi",
                                    Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                                    Language = Language.Spanish,
                                    Series = _series
                                };
        }

        [Test]
        public void should_return_true_if_no_existing_episodeFile()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 0)
                                                     .With(e => e.EpisodeFile = null)
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_no_existing_episodeFile_for_multi_episodes()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 0)
                                                     .With(e => e.EpisodeFile = null)
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_upgrade_for_existing_episodeFile()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                                                new EpisodeFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.SDTV, new Revision(version: 1)),
                                                                                    Language = Language.Spanish
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_language_upgrade_for_existing_episodeFile_and_quality_is_same()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                                                new EpisodeFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                                                                                    Language = Language.English
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_language_upgrade_for_existing_episodeFile_and_quality_is_same_but_lower_revision()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                .All()
                .With(e => e.EpisodeFileId = 1)
                .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                    new EpisodeFile
                    {
                        Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 2)),
                        Language = Language.English
                    }))
                .Build()
                .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_language_upgrade_for_existing_episodeFile_and_quality_is_worse()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                                                new EpisodeFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 1)),
                                                                                    Language = Language.English
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_upgrade_for_existing_episodeFile_for_multi_episodes()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                                                new EpisodeFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.SDTV, new Revision(version: 1))
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_language_upgrade_for_existing_episodeFile_for_multi_episodes_and_quality_is_same()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                                                new EpisodeFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                                                                                    Language = Language.English
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_language_upgrade_for_existing_episodeFile_for_multi_episodes_and_quality_is_worse()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                                                new EpisodeFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 1)),
                                                                                    Language = Language.English
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_not_an_upgrade_for_existing_episodeFile()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                                                new EpisodeFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.Bluray720p, new Revision(version: 1))
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_not_an_upgrade_for_existing_episodeFile_for_multi_episodes()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                                                new EpisodeFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.Bluray720p, new Revision(version: 1))
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_not_an_upgrade_for_one_existing_episodeFile_for_multi_episode()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(2)
                                                     .TheFirst(1)
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                                                new EpisodeFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.SDTV, new Revision(version: 1))
                                                                                }))
                                                     .TheNext(1)
                                                     .With(e => e.EpisodeFileId = 2)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                                                new EpisodeFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.Bluray720p, new Revision(version: 1))
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_not_a_revision_upgrade_and_prefers_propers()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.PreferAndUpgrade);

            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                         new EpisodeFile
                                                         {
                                                             Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 2)),
                                                             Language = Language.Spanish
                                                         }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_it_is_a_preferred_word_downgrade_and_equal_language_and_quality()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<IEpisodeFilePreferredWordCalculator>()
                  .Setup(s => s.Calculate(It.IsAny<Series>(), It.IsAny<EpisodeFile>()))
                  .Returns(10);

            _localEpisode.PreferredWordScore = 5;
            _localEpisode.Quality = new QualityModel(Quality.Bluray1080p);

            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                         new EpisodeFile
                                                         {
                                                             Quality = new QualityModel(Quality.Bluray1080p),
                                                             Language = Language.Spanish
                                                         }))
                                                     .Build()
                                                     .ToList();

            _localEpisode.FileEpisodeInfo = Builder<ParsedEpisodeInfo>.CreateNew().Build();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_it_is_a_preferred_word_downgrade_and_language_downgrade_and_a_quality_upgrade()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<IEpisodeFilePreferredWordCalculator>()
                  .Setup(s => s.Calculate(It.IsAny<Series>(), It.IsAny<EpisodeFile>()))
                  .Returns(10);

            _localEpisode.PreferredWordScore = 5;
            _localEpisode.Quality = new QualityModel(Quality.Bluray2160p);

            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                         new EpisodeFile
                                                         {
                                                             Quality = new QualityModel(Quality.Bluray1080p),
                                                             Language = Language.French
                                                         }))
                                                     .Build()
                                                     .ToList();

            _localEpisode.FileEpisodeInfo = Builder<ParsedEpisodeInfo>.CreateNew().Build();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_it_is_a_preferred_word_downgrade_but_a_language_upgrade()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<IEpisodeFilePreferredWordCalculator>()
                  .Setup(s => s.Calculate(It.IsAny<Series>(), It.IsAny<EpisodeFile>()))
                  .Returns(10);

            _localEpisode.PreferredWordScore = 5;
            _localEpisode.Quality = new QualityModel(Quality.Bluray1080p);

            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                         new EpisodeFile
                                                         {
                                                             Quality = new QualityModel(Quality.Bluray1080p),
                                                             Language = Language.English
                                                         }))
                                                     .Build()
                                                     .ToList();

            _localEpisode.FileEpisodeInfo = Builder<ParsedEpisodeInfo>.CreateNew().Build();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_not_a_revision_upgrade_and_does_not_prefer_propers()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                         new EpisodeFile
                                                         {
                                                             Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 2))
                                                         }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_comparing_to_a_lower_quality_proper()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            _localEpisode.Quality = new QualityModel(Quality.Bluray1080p);

            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                         new EpisodeFile
                                                         {
                                                             Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 2))
                                                         }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_it_is_a_preferred_word_upgrade()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<IEpisodeFilePreferredWordCalculator>()
                  .Setup(s => s.Calculate(It.IsAny<Series>(), It.IsAny<EpisodeFile>()))
                  .Returns(1);

            _localEpisode.PreferredWordScore = 5;
            _localEpisode.Quality = new QualityModel(Quality.Bluray1080p);

            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                         new EpisodeFile
                                                         {
                                                             Quality = new QualityModel(Quality.Bluray1080p)
                                                         }))
                                                     .Build()
                                                     .ToList();

            _localEpisode.FileEpisodeInfo = Builder<ParsedEpisodeInfo>.CreateNew().Build();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_it_has_an_equal_preferred_word_score()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<IEpisodeFilePreferredWordCalculator>()
                  .Setup(s => s.Calculate(It.IsAny<Series>(), It.IsAny<EpisodeFile>()))
                  .Returns(5);

            _localEpisode.PreferredWordScore = 5;
            _localEpisode.Quality = new QualityModel(Quality.Bluray1080p);

            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                         new EpisodeFile
                                                         {
                                                             Quality = new QualityModel(Quality.Bluray1080p)
                                                         }))
                                                     .Build()
                                                     .ToList();

            _localEpisode.FileEpisodeInfo = Builder<ParsedEpisodeInfo>.CreateNew().Build();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_episode_file_is_null()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(null))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }
    }
}
