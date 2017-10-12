using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications.RssSync;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.DecisionEngine;

using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests.RssSync
{
    [TestFixture]

    public class ProperSpecificationFixture : CoreTest<ProperSpecification>
    {
        private RemoteEpisode _parseResultMulti;
        private RemoteEpisode _parseResultSingle;
        private EpisodeFile _firstFile;
        private EpisodeFile _secondFile;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();

            _firstFile = new EpisodeFile { Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 1)), DateAdded = DateTime.Now };
            _secondFile = new EpisodeFile { Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 1)), DateAdded = DateTime.Now };

            var singleEpisodeList = new List<Episode> { new Episode { EpisodeFile = _firstFile, EpisodeFileId = 1 }, new Episode { EpisodeFile = null } };
            var doubleEpisodeList = new List<Episode> { new Episode { EpisodeFile = _firstFile, EpisodeFileId = 1 }, new Episode { EpisodeFile = _secondFile, EpisodeFileId = 1 }, new Episode { EpisodeFile = null } };

            var fakeSeries = Builder<Series>.CreateNew()
                         .With(c => c.Profile = new Profile { Cutoff = Quality.Bluray1080p.Id })
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
        }

        private void WithFirstFileUpgradable()
        {
            _firstFile.Quality = new QualityModel(Quality.SDTV);
        }

        private void GivenAutoDownloadPropers()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.AutoDownloadPropers)
                  .Returns(true);
        }

        [Test]
        public void should_return_false_when_episodeFile_was_added_more_than_7_days_ago()
        {
            _firstFile.Quality.Quality = Quality.DVD;

            _firstFile.DateAdded = DateTime.Today.AddDays(-30);
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_first_episodeFile_was_added_more_than_7_days_ago()
        {
            _firstFile.Quality.Quality = Quality.DVD;
            _secondFile.Quality.Quality = Quality.DVD;

            _firstFile.DateAdded = DateTime.Today.AddDays(-30);
            Subject.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_second_episodeFile_was_added_more_than_7_days_ago()
        {
            _firstFile.Quality.Quality = Quality.DVD;
            _secondFile.Quality.Quality = Quality.DVD;

            _secondFile.DateAdded = DateTime.Today.AddDays(-30);
            Subject.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_episodeFile_was_added_more_than_7_days_ago_but_proper_is_for_better_quality()
        {
            WithFirstFileUpgradable();

            _firstFile.DateAdded = DateTime.Today.AddDays(-30);
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_episodeFile_was_added_more_than_7_days_ago_but_is_for_search()
        {
            WithFirstFileUpgradable();

            _firstFile.DateAdded = DateTime.Today.AddDays(-30);
            Subject.IsSatisfiedBy(_parseResultSingle, new SingleEpisodeSearchCriteria()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_proper_but_auto_download_propers_is_false()
        {
            _firstFile.Quality.Quality = Quality.DVD;

            _firstFile.DateAdded = DateTime.Today;
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_episodeFile_was_added_today()
        {
            GivenAutoDownloadPropers();

            _firstFile.Quality.Quality = Quality.DVD;

            _firstFile.DateAdded = DateTime.Today;
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }
    }
}