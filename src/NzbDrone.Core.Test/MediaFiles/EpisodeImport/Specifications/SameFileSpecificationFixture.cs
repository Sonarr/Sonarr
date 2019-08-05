using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Marr.Data;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class SameFileSpecificationFixture : CoreTest<SameFileSpecification>
    {
        private LocalEpisode _localEpisode;

        [SetUp]
        public void Setup()
        {
            _localEpisode = Builder<LocalEpisode>.CreateNew()
                                                 .With(l => l.Size = 150.Megabytes())
                                                 .Build();
        }

        [Test]
        public void should_be_accepted_if_no_existing_file()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .TheFirst(1)
                                                     .With(e => e.EpisodeFileId = 0)
                                                     .BuildList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_multiple_existing_files()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(2)
                                                     .TheFirst(1)
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                                                new EpisodeFile
                                                                                {
                                                                                    Size = _localEpisode.Size
                                                                                }))
                                                     .TheNext(1)
                                                     .With(e => e.EpisodeFileId = 2)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                                                new EpisodeFile
                                                                                {
                                                                                    Size = _localEpisode.Size
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_file_size_is_different()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .TheFirst(1)
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                                                new EpisodeFile
                                                                                {
                                                                                    Size = _localEpisode.Size + 100.Megabytes()
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_reject_if_file_size_is_the_same()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .TheFirst(1)
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                                                new EpisodeFile
                                                                                {
                                                                                    Size = _localEpisode.Size
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_accepted_if_file_cannot_be_fetched()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                .TheFirst(1)
                .With(e => e.EpisodeFileId = 1)
                .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>((EpisodeFile)null))
                .Build()
                .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }
    }
}