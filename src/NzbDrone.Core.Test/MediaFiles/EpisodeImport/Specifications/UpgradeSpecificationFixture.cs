using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Marr.Data;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Languages;

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
                                     .With(e => e.Profile = new Profile 
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
    }
}
