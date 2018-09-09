using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Internal;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]
    public class IdFixture : CoreTest<FileNameBuilder>
    {
        private Series _series;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>
                      .CreateNew()
                      .With(s => s.Title = "Series Title")
                      .With(s => s.ImdbId = "tt12345")
                      .With(s => s.TvdbId = 12345)
                      .With(s => s.TvRageId = 54321)
                      .Build();

            _namingConfig = NamingConfig.Default;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);
        }

        [Test]
        public void should_add_imdb_id()
        {
            _namingConfig.SeriesFolderFormat = "{Series Title} ({ImdbId})";

            Subject.GetSeriesFolder(_series)
                   .Should().Be($"Series Title ({_series.ImdbId})");
        }

        [Test]
        public void should_add_tvdb_id()
        {
            _namingConfig.SeriesFolderFormat = "{Series Title} ({TvdbId})";

            Subject.GetSeriesFolder(_series)
                   .Should().Be($"Series Title ({_series.TvdbId})");
        }

        [Test]
        public void should_add_tvmaze_id()
        {
            _namingConfig.SeriesFolderFormat = "{Series Title} ({TvMazeId})";

            Subject.GetSeriesFolder(_series)
                   .Should().Be($"Series Title ({_series.TvMazeId})");
        }
    }
}
