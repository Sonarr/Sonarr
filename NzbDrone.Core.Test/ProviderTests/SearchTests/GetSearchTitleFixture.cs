using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Search;
using NzbDrone.Core.Repository;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.SearchTests
{
    public class GetSearchTitleFixture : TestBase
    {
        private Series _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>
                    .CreateNew()
                    .With(s => s.Title = "Hawaii Five-0")
                    .Build();
        }

        private void WithSceneMapping()
        {
            Mocker.GetMock<SceneMappingProvider>()
                  .Setup(s => s.GetSceneName(_series.SeriesId))
                  .Returns("Hawaii Five 0 2010");
        }

        [Test]
        public void should_return_series_title_when_there_is_no_scene_mapping()
        {
            Mocker.Resolve<TestSearch>().GetSearchTitle(_series, 5)
                  .Should().Be(_series.Title);
        }

        [Test]
        public void should_return_scene_mapping_when_one_exists()
        {
            WithSceneMapping();

            Mocker.Resolve<TestSearch>().GetSearchTitle(_series, 5)
                  .Should().Be("Hawaii Five 0 2010");
        }

        [Test]
        public void should_replace_ampersand_with_and()
        {
            _series.Title = "Franklin & Bash";

            Mocker.Resolve<TestSearch>().GetSearchTitle(_series, 5)
                  .Should().Be("Franklin and Bash");
        }
    }
}
