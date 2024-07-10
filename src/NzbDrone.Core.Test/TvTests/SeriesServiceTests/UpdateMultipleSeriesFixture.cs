using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.AutoTagging;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
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
                .Build().ToList();

            Mocker.GetMock<IAutoTaggingService>()
                .Setup(s => s.GetTagChanges(It.IsAny<Series>()))
                .Returns(new AutoTaggingChanges());
        }

        [Test]
        public void should_call_repo_updateMany()
        {
            Subject.UpdateSeries(_series, false);

            Mocker.GetMock<ISeriesRepository>().Verify(v => v.UpdateMany(_series), Times.Once());
        }

        [Test]
        public void should_update_path_when_rootFolderPath_is_supplied()
        {
            var newRoot = @"C:\Test\TV2".AsOsAgnostic();
            _series.ForEach(s => s.RootFolderPath = newRoot);

            Mocker.GetMock<IBuildSeriesPaths>()
                  .Setup(s => s.BuildPath(It.IsAny<Series>(), false))
                  .Returns<Series, bool>((s, u) => Path.Combine(s.RootFolderPath, s.Title));

            Subject.UpdateSeries(_series, false).ForEach(s => s.Path.Should().StartWith(newRoot));
        }

        [Test]
        public void should_not_update_path_when_rootFolderPath_is_empty()
        {
            Subject.UpdateSeries(_series, false).ForEach(s =>
            {
                var expectedPath = _series.Single(ser => ser.Id == s.Id).Path;
                s.Path.Should().Be(expectedPath);
            });
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
    }
}
