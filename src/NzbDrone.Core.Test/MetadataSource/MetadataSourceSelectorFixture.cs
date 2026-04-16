using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.MetadataSource
{
    [TestFixture]
    public class MetadataSourceSelectorFixture : CoreTest<MetadataSourceSelector>
    {
        [Test]
        public void should_use_tvdb_provider_for_series_search_when_tvdb_selected()
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(c => c.MetadataSource)
                  .Returns(MetadataSourceType.Tvdb);

            var expected = new List<Series> { new Series { TvdbId = 1 } };

            Mocker.GetMock<ITvdbMetadataSource>()
                  .Setup(s => s.SearchForNewSeries("archer"))
                  .Returns(expected);

            Subject.SearchForNewSeries("archer").Should().BeSameAs(expected);

            Mocker.GetMock<ITmdbMetadataSource>()
                  .Verify(s => s.SearchForNewSeries(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_use_tmdb_provider_for_series_search_when_tmdb_selected()
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(c => c.MetadataSource)
                  .Returns(MetadataSourceType.Tmdb);

            var expected = new List<Series> { new Series { TmdbId = 2 } };

            Mocker.GetMock<ITmdbMetadataSource>()
                  .Setup(s => s.SearchForNewSeries("archer"))
                  .Returns(expected);

            Subject.SearchForNewSeries("archer").Should().BeSameAs(expected);

            Mocker.GetMock<ITvdbMetadataSource>()
                  .Verify(s => s.SearchForNewSeries(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_always_use_tvdb_provider_for_anilist_search()
        {
            var expected = new List<Series> { new Series { TvdbId = 3 } };

            Mocker.GetMock<ITvdbMetadataSource>()
                  .Setup(s => s.SearchForNewSeriesByAniListId(123))
                  .Returns(expected);

            Subject.SearchForNewSeriesByAniListId(123).Should().BeSameAs(expected);

            Mocker.GetMock<ITmdbMetadataSource>()
                  .Verify(s => s.SearchForNewSeriesByAniListId(It.IsAny<int>()), Times.Never());
        }

        [Test]
        public void should_use_tmdb_provider_for_series_info_when_tmdb_selected()
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(c => c.MetadataSource)
                  .Returns(MetadataSourceType.Tmdb);

            var expected = new Tuple<Series, List<Episode>>(new Series { TmdbId = 10 }, new List<Episode>());

            Mocker.GetMock<ITmdbMetadataSource>()
                  .Setup(s => s.GetSeriesInfo(20, 10))
                  .Returns(expected);

            Subject.GetSeriesInfo(20, 10).Should().BeSameAs(expected);
        }
    }
}
