using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.SearchProviderTests
{
    public class GetSeriesTitleFixture : TestBase
    {
        private Series _series;
        private const string SCENE_NAME = "Scandal";

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>
                .CreateNew()
                .With(s => s.Title = "Scandal (2012)")
                .Build();
        }

        private void WithSceneName()
        {
            Mocker.GetMock<SceneMappingProvider>()
                  .Setup(s => s.GetSceneName(_series.SeriesId))
                  .Returns("Scandal");
        }

        [Test]
        public void should_return_scene_name_when_sceneName_is_available()
        {
            WithSceneName();

            Mocker.Resolve<SearchProvider>().GetSeriesTitle(_series).Should().Be(SCENE_NAME);
        }

        [Test]
        public void should_return_seriesTitle_when_sceneName_is_not_available()
        {
            Mocker.Resolve<SearchProvider>().GetSeriesTitle(_series).Should().Be(_series.Title);
        }

        [TestCase("Mike & Molly", "Mike and Molly")]
        [TestCase("Franklin & Bash", "Franklin and Bash")]
        [TestCase("Law & Order", "Law and Order")]
        public void should_replace_ampersand_with_and_when_returning_title(string seriesTitle, string expectedTitle)
        {
            _series.Title = seriesTitle;

            Mocker.Resolve<SearchProvider>().GetSeriesTitle(_series).Should().Be(expectedTitle);
        }
    }
}
