using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.EpisodeImport.Manual;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Update.Commands;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Messaging.Commands
{
    [TestFixture]
    public class CommandEqualityComparerFixture
    {
        private string GivenRandomPath()
        {
            return Path.Combine(@"C:\Tesst\", Guid.NewGuid().ToString()).AsOsAgnostic();
        }

        [Test]
        public void should_return_true_when_there_are_no_properties()
        {
            var command1 = new DownloadedEpisodesScanCommand();
            var command2 = new DownloadedEpisodesScanCommand();

            CommandEqualityComparer.Instance.Equals(command1, command2).Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_single_property_matches()
        {
            var command1 = new EpisodeSearchCommand { EpisodeIds = new List<int> { 1 } };
            var command2 = new EpisodeSearchCommand { EpisodeIds = new List<int> { 1 } };

            CommandEqualityComparer.Instance.Equals(command1, command2).Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_multiple_properties_match()
        {
            var command1 = new SeasonSearchCommand { SeriesId = 1, SeasonNumber = 1 };
            var command2 = new SeasonSearchCommand { SeriesId = 1, SeasonNumber = 1 };

            CommandEqualityComparer.Instance.Equals(command1, command2).Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_single_property_doesnt_match()
        {
            var command1 = new EpisodeSearchCommand { EpisodeIds = new List<int> { 1 } };
            var command2 = new EpisodeSearchCommand { EpisodeIds = new List<int> { 2 } };

            CommandEqualityComparer.Instance.Equals(command1, command2).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_only_one_property_matches()
        {
            var command1 = new SeasonSearchCommand { SeriesId = 1, SeasonNumber = 1 };
            var command2 = new SeasonSearchCommand { SeriesId = 1, SeasonNumber = 2 };

            CommandEqualityComparer.Instance.Equals(command1, command2).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_no_properties_match()
        {
            var command1 = new SeasonSearchCommand { SeriesId = 1, SeasonNumber = 1 };
            var command2 = new SeasonSearchCommand { SeriesId = 2, SeasonNumber = 2 };

            CommandEqualityComparer.Instance.Equals(command1, command2).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_only_one_has_properties()
        {
            var command1 = new SeasonSearchCommand();
            var command2 = new SeasonSearchCommand { SeriesId = 2, SeasonNumber = 2 };

            CommandEqualityComparer.Instance.Equals(command1, command2).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_only_one_has_null_property()
        {
            var command1 = new EpisodeSearchCommand(null);
            var command2 = new EpisodeSearchCommand(new List<int>());

            CommandEqualityComparer.Instance.Equals(command1, command2).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_commands_are_diffrent_types()
        {
            CommandEqualityComparer.Instance.Equals(new RssSyncCommand(), new ApplicationUpdateCommand()).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_commands_list_are_different_lengths()
        {
            var command1 = new EpisodeSearchCommand { EpisodeIds = new List<int> { 1 } };
            var command2 = new EpisodeSearchCommand { EpisodeIds = new List<int> { 1, 2 } };

            CommandEqualityComparer.Instance.Equals(command1, command2).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_commands_list_dont_match()
        {
            var command1 = new EpisodeSearchCommand { EpisodeIds = new List<int> { 1 } };
            var command2 = new EpisodeSearchCommand { EpisodeIds = new List<int> { 2 } };

            CommandEqualityComparer.Instance.Equals(command1, command2).Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_commands_list_for_non_primitive_type_match()
        {
            var files1 = Builder<ManualImportFile>.CreateListOfSize(2)
                                                  .All()
                                                  .With(m => m.Path = GivenRandomPath())
                                                  .Build()
                                                  .ToList();

            var files2 = files1.JsonClone();

            var command1 = new ManualImportCommand { Files = files1 };
            var command2 = new ManualImportCommand { Files = files2 };

            CommandEqualityComparer.Instance.Equals(command1, command2).Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_commands_list_for_non_primitive_type_dont_match()
        {
            var files1 = Builder<ManualImportFile>.CreateListOfSize(2)
                                                  .All()
                                                  .With(m => m.Path = GivenRandomPath())
                                                  .Build()
                                                  .ToList();

            var files2 = Builder<ManualImportFile>.CreateListOfSize(2)
                                                  .All()
                                                  .With(m => m.Path = GivenRandomPath())
                                                  .Build()
                                                  .ToList();

            var command1 = new ManualImportCommand { Files = files1 };
            var command2 = new ManualImportCommand { Files = files2 };

            CommandEqualityComparer.Instance.Equals(command1, command2).Should().BeFalse();
        }
    }
}
