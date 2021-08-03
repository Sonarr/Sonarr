using System;
using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications.RssSync;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.DecisionEngineTests.RssSync
{
    [TestFixture]
    public class DeletedEpisodeFileSpecificationFixture : CoreTest<DeletedEpisodeFileSpecification>
    {
        private RemoteEpisode _parseResultMulti;
        private RemoteEpisode _parseResultSingle;
        private EpisodeFile _firstFile;
        private EpisodeFile _secondFile;

        [SetUp]
        public void Setup()
        {
            _firstFile = new EpisodeFile
            {
                Id = 1,
                RelativePath = "My.Series.S01E01.mkv",
                Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 1)),
                DateAdded = DateTime.Now
            };
            _secondFile = new EpisodeFile
            {
                Id = 2,
                RelativePath = "My.Series.S01E02.mkv",
                Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 1)),
                DateAdded = DateTime.Now
            };

            var singleEpisodeList = new List<Episode> { new Episode { EpisodeFile = _firstFile, EpisodeFileId = 1 } };
            var doubleEpisodeList = new List<Episode>
            {
                new Episode { EpisodeFile = _firstFile, EpisodeFileId = 1 },
                new Episode { EpisodeFile = _secondFile, EpisodeFileId = 2 }
            };

            var fakeSeries = Builder<Series>.CreateNew()
                         .With(c => c.QualityProfile = new QualityProfile { Cutoff = Quality.Bluray1080p.Id })
                         .With(c => c.Path = @"C:\Series\My.Series".AsOsAgnostic())
                         .Build();

            _parseResultMulti = new RemoteEpisode
            {
                Series = fakeSeries,
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)) },
                Episodes = doubleEpisodeList
            };

            _parseResultSingle = new RemoteEpisode
            {
                Series = fakeSeries,
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)) },
                Episodes = singleEpisodeList
            };

            GivenUnmonitorDeletedEpisodes(true);
        }

        private void GivenUnmonitorDeletedEpisodes(bool enabled)
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(v => v.AutoUnmonitorPreviouslyDownloadedEpisodes)
                  .Returns(enabled);
        }

        private void WithExistingFile(EpisodeFile episodeFile)
        {
            var path = Path.Combine(@"C:\Series\My.Series".AsOsAgnostic(), episodeFile.RelativePath);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FileExists(path))
                  .Returns(true);
        }

        [Test]
        public void should_return_true_when_unmonitor_deleted_episdes_is_off()
        {
            GivenUnmonitorDeletedEpisodes(false);

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_searching()
        {
            Subject.IsSatisfiedBy(_parseResultSingle, new SeasonSearchCriteria()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_file_exists()
        {
            WithExistingFile(_firstFile);

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_file_is_missing()
        {
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_both_of_multiple_episode_exist()
        {
            WithExistingFile(_firstFile);
            WithExistingFile(_secondFile);

            Subject.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_one_of_multiple_episode_is_missing()
        {
            WithExistingFile(_firstFile);

            Subject.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }
    }
}
