using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class MatchesFolderSpecificationFixture : CoreTest<MatchesFolderSpecification>
    {
        private LocalEpisode _localEpisode;

        [SetUp]
        public void Setup()
        {
            _localEpisode = Builder<LocalEpisode>.CreateNew()
                                                 .With(l => l.Path = @"C:\Test\Unsorted\Series.Title.S01E01.720p.HDTV-Sonarr\S01E05.mkv".AsOsAgnostic())
                                                 .With(l => l.FileEpisodeInfo =
                                                     Builder<ParsedEpisodeInfo>.CreateNew()
                                                                               .With(p => p.EpisodeNumbers = new[] {5})
                                                                               .With(p => p.FullSeason = false)
                                                                               .Build())
                                                 .Build();
        }

        [Test]
        public void should_be_accepted_for_existing_file()
        {
            _localEpisode.ExistingFile = true;

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_folder_name_is_not_parseable()
        {
            _localEpisode.Path = @"C:\Test\Unsorted\Series.Title\S01E01.mkv".AsOsAgnostic();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_should_be_accepted_for_full_season()
        {
            _localEpisode.Path = @"C:\Test\Unsorted\Series.Title.S01\S01E01.mkv".AsOsAgnostic();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_file_and_folder_have_the_same_episode()
        {
            _localEpisode.FileEpisodeInfo.EpisodeNumbers = new[] { 1 };
            _localEpisode.FolderEpisodeInfo.EpisodeNumbers = new[] { 1 };
            _localEpisode.Path = @"C:\Test\Unsorted\Series.Title.S01E01.720p.HDTV-Sonarr\S01E01.mkv".AsOsAgnostic();
            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_file_is_one_episode_in_folder()
        {
            _localEpisode.FileEpisodeInfo.EpisodeNumbers = new[] { 1 };
            _localEpisode.FolderEpisodeInfo.EpisodeNumbers = new[] { 1 };
            _localEpisode.Path = @"C:\Test\Unsorted\Series.Title.S01E01E02.720p.HDTV-Sonarr\S01E01.mkv".AsOsAgnostic();
            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();            
        }

        [Test]
        public void should_be_rejected_if_file_and_folder_do_not_have_same_episode()
        {
            _localEpisode.Path = @"C:\Test\Unsorted\Series.Title.S01E01.720p.HDTV-Sonarr\S01E05.mkv".AsOsAgnostic();
            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();            
        }

        [Test]
        public void should_be_rejected_if_file_and_folder_do_not_have_same_episodes()
        {
            _localEpisode.FileEpisodeInfo.EpisodeNumbers = new[] { 5, 6 };
            _localEpisode.FolderEpisodeInfo.EpisodeNumbers = new[] { 1, 2 };
            _localEpisode.Path = @"C:\Test\Unsorted\Series.Title.S01E01E02.720p.HDTV-Sonarr\S01E05E06.mkv".AsOsAgnostic();
            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
        }
    }
}
