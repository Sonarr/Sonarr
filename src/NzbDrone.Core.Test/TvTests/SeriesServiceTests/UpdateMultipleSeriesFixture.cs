using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.AutoTagging;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Commands;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.TvTests.SeriesServiceTests
{
    [TestFixture]
    public class UpdateMultipleSeriesFixture : CoreTest<SeriesService>
    {
        private List<Series> _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateListOfSize(5)
                .All()
                .With(s => s.QualityProfileId = 1)
                .With(s => s.Monitored)
                .With(s => s.SeasonFolder)
                .With(s => s.Path = @"C:\Test\name".AsOsAgnostic())
                .With(s => s.RootFolderPath = "")
                .With(s => s.PendingPath = null)
                .Build().ToList();

            Mocker.GetMock<IAutoTaggingService>()
                .Setup(s => s.GetTagChanges(It.IsAny<Series>()))
                .Returns(new AutoTaggingChanges());

            Mocker.GetMock<IManageCommandQueue>()
                .Setup(s => s.All())
                .Returns(new List<CommandModel>());
        }

        private void GivenPendingMove(IEnumerable<int> seriesIds)
        {
            Mocker.GetMock<IManageCommandQueue>()
                .Setup(s => s.All())
                .Returns(new List<CommandModel>
                {
                    new CommandModel
                    {
                        Status = CommandStatus.Queued,
                        Body = new BulkMoveSeriesCommand
                        {
                            Series = seriesIds.Select(id => new BulkMoveSeries { SeriesId = id }).ToList()
                        }
                    }
                });
        }

        [Test]
        public void should_call_repo_updateMany()
        {
            Subject.UpdateSeries(_series, true);

            Mocker.GetMock<ISeriesRepository>().Verify(v => v.UpdateMany(_series), Times.Once());
        }

        [Test]
        public void should_update_path_when_rootFolderPath_is_supplied()
        {
            var newRoot = @"C:\Test\TV2".AsOsAgnostic();
            _series.ForEach(s => s.RootFolderPath = newRoot);

            Mocker.GetMock<IBuildSeriesPaths>()
                  .Setup(s => s.BuildPath(It.IsAny<Series>(), true))
                  .Returns<Series, bool>((s, u) => Path.Combine(s.RootFolderPath, s.Title));

            Subject.UpdateSeries(_series, false).ForEach(s => s.Path.Should().StartWith(newRoot));
        }

        [Test]
        public void should_not_update_path_directly_when_deferring()
        {
            var newRoot = @"C:\Test\TV2".AsOsAgnostic();
            _series.ForEach(s => s.RootFolderPath = newRoot);

            Mocker.GetMock<IBuildSeriesPaths>()
                  .Setup(s => s.BuildPath(It.IsAny<Series>(), false))
                  .Returns<Series, bool>((s, u) => Path.Combine(s.RootFolderPath, s.Title));

            var originalPaths = _series.ToDictionary(s => s.Id, s => s.Path);

            var result = Subject.UpdateSeries(_series, true);

            result.Should().OnlyContain(s => s.Path == originalPaths[s.Id]);
        }

        [Test]
        public void should_not_update_path_when_deferred_and_rootFolderPath_is_empty()
        {
            var result = Subject.UpdateSeries(_series, true);

            result.ForEach(s =>
            {
                var expectedPath = _series.Single(ser => ser.Id == s.Id).Path;
                s.Path.Should().Be(expectedPath);
            });

            Mocker.GetMock<IBuildSeriesPaths>().Verify(v => v.BuildPath(It.IsAny<Series>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_be_able_to_update_many_series()
        {
            var series = Builder<Series>.CreateListOfSize(50)
                                        .All()
                                        .With(s => s.Path = (@"C:\Test\TV\" + s.Path).AsOsAgnostic())
                                        .Build()
                                        .ToList();

            var newRoot = @"C:\Test\TV2".AsOsAgnostic();
            series.ForEach(s => s.RootFolderPath = newRoot);

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.GetSeriesFolder(It.IsAny<Series>(), (NamingConfig)null))
                  .Returns<Series, NamingConfig>((s, n) => s.Title);

            Subject.UpdateSeries(series, false);
        }

        [Test]
        public void should_add_and_remove_tags()
        {
            _series[0].Tags = new HashSet<int> { 1, 2 };

            Mocker.GetMock<IAutoTaggingService>()
                .Setup(s => s.GetTagChanges(_series[0]))
                .Returns(new AutoTaggingChanges
                {
                    TagsToAdd = new HashSet<int> { 3 },
                    TagsToRemove = new HashSet<int> { 1 }
                });

            var result = Subject.UpdateSeries(_series, false);

            result[0].Tags.Should().BeEquivalentTo(new[] { 2, 3 });
        }

        [Test]
        public void should_not_overwrite_pending_path_when_a_move_is_already_pending()
        {
            var newRoot = @"C:\Test\TV2".AsOsAgnostic();
            var existingPendingPath = @"C:\Test\TV3\name".AsOsAgnostic();

            _series.ForEach(s =>
            {
                s.RootFolderPath = newRoot;
                s.PendingPath = existingPendingPath;
            });

            GivenPendingMove(_series.Select(s => s.Id));

            var result = Subject.UpdateSeries(_series, true);

            ExceptionVerification.ExpectedWarns(_series.Count);

            result.Should().OnlyContain(s => s.PendingPath == existingPendingPath);
            Mocker.GetMock<IBuildSeriesPaths>().Verify(v => v.BuildPath(It.IsAny<Series>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_set_pending_path_when_deferring()
        {
            var newRoot = @"C:\Test\TV2".AsOsAgnostic();
            _series.ForEach(s => s.RootFolderPath = newRoot);

            Mocker.GetMock<IBuildSeriesPaths>()
                  .Setup(s => s.BuildPath(It.IsAny<Series>(), false))
                  .Returns<Series, bool>((s, u) => Path.Combine(s.RootFolderPath, s.Title));

            var result = Subject.UpdateSeries(_series, true);

            result.Should().OnlyContain(s => s.PendingPath != null && s.PendingPath.StartsWith(newRoot));
        }

        [Test]
        public void should_push_bulk_move_command_when_deferring()
        {
            var newRoot = @"C:\Test\TV2".AsOsAgnostic();
            _series.ForEach(s => s.RootFolderPath = newRoot);

            Mocker.GetMock<IBuildSeriesPaths>()
                  .Setup(s => s.BuildPath(It.IsAny<Series>(), false))
                  .Returns<Series, bool>((s, u) => Path.Combine(s.RootFolderPath, s.Title));

            Subject.UpdateSeries(_series, true);

            Mocker.GetMock<IManageCommandQueue>()
                .Verify(v => v.Push(It.Is<BulkMoveSeriesCommand>(c => c.Series.Select(m => m.SeriesId).ToList().SequenceEqual(_series.Select(s => s.Id).ToList())), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()), Times.Once());
        }

        [Test]
        public void should_not_push_bulk_move_command_when_a_move_is_already_pending()
        {
            var newRoot = @"C:\Test\TV2".AsOsAgnostic();
            _series.ForEach(s =>
            {
                s.RootFolderPath = newRoot;
                s.PendingPath = @"C:\Test\TV3\name".AsOsAgnostic();
            });

            GivenPendingMove(_series.Select(s => s.Id));

            Subject.UpdateSeries(_series, true);

            ExceptionVerification.ExpectedWarns(_series.Count);

            Mocker.GetMock<IManageCommandQueue>()
                .Verify(v => v.Push(It.IsAny<BulkMoveSeriesCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()), Times.Never());
        }

        [Test]
        public void should_not_push_bulk_move_command_when_not_deferring()
        {
            var newRoot = @"C:\Test\TV2".AsOsAgnostic();
            _series.ForEach(s => s.RootFolderPath = newRoot);

            Mocker.GetMock<IBuildSeriesPaths>()
                  .Setup(s => s.BuildPath(It.IsAny<Series>(), true))
                  .Returns<Series, bool>((s, u) => Path.Combine(s.RootFolderPath, s.Title));

            Subject.UpdateSeries(_series, false);

            Mocker.GetMock<IManageCommandQueue>()
                .Verify(v => v.Push(It.IsAny<BulkMoveSeriesCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()), Times.Never());
        }

        [Test]
        public void should_not_touch_path_or_pending_path_for_series_with_genuine_pending_move_when_not_moving_files()
        {
            var pendingPath = @"C:\Test\TV3\name".AsOsAgnostic();
            _series.ForEach(s => s.PendingPath = pendingPath);
            GivenPendingMove(_series.Select(s => s.Id));

            var originalPaths = _series.ToDictionary(s => s.Id, s => s.Path);

            _series[0].Tags = new HashSet<int> { 1 };
            Mocker.GetMock<IAutoTaggingService>()
                .Setup(s => s.GetTagChanges(_series[0]))
                .Returns(new AutoTaggingChanges { TagsToAdd = new HashSet<int> { 2 } });

            var result = Subject.UpdateSeries(_series, false);

            result.Should().OnlyContain(s => s.Path == originalPaths[s.Id] && s.PendingPath == pendingPath);
            result[0].Tags.Should().Contain(2);
        }
    }
}
