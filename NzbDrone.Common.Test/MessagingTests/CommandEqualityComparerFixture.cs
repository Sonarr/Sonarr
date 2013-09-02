using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.MediaFiles.Commands;

namespace NzbDrone.Common.Test.MessagingTests
{
    [TestFixture]
    public class CommandEqualityComparerFixture
    {
        [Test]
        public void should_return_true_when_there_are_no_properties()
        {
            var command1 = new DownloadedEpisodesScanCommand();
            var command2 = new DownloadedEpisodesScanCommand();
            var comparer = new CommandEqualityComparer();

            comparer.Equals(command1, command2).Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_single_property_matches()
        {
            var command1 = new EpisodeSearchCommand { EpisodeId = 1 };
            var command2 = new EpisodeSearchCommand { EpisodeId = 1 };
            var comparer = new CommandEqualityComparer();

            comparer.Equals(command1, command2).Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_multiple_properties_match()
        {
            var command1 = new SeasonSearchCommand { SeriesId = 1, SeasonNumber = 1 };
            var command2 = new SeasonSearchCommand { SeriesId = 1, SeasonNumber = 1 };
            var comparer = new CommandEqualityComparer();

            comparer.Equals(command1, command2).Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_single_property_doesnt_match()
        {
            var command1 = new EpisodeSearchCommand { EpisodeId = 1 };
            var command2 = new EpisodeSearchCommand { EpisodeId = 2 };
            var comparer = new CommandEqualityComparer();

            comparer.Equals(command1, command2).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_only_one_property_matches()
        {
            var command1 = new SeasonSearchCommand { SeriesId = 1, SeasonNumber = 1 };
            var command2 = new SeasonSearchCommand { SeriesId = 1, SeasonNumber = 2 };
            var comparer = new CommandEqualityComparer();

            comparer.Equals(command1, command2).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_no_properties_match()
        {
            var command1 = new SeasonSearchCommand { SeriesId = 1, SeasonNumber = 1 };
            var command2 = new SeasonSearchCommand { SeriesId = 2, SeasonNumber = 2 };
            var comparer = new CommandEqualityComparer();

            comparer.Equals(command1, command2).Should().BeFalse();
        }
    }
}
