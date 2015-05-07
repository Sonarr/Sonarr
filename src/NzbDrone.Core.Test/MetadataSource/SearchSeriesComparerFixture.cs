using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.MetadataSource
{
    [TestFixture]
    public class SearchSeriesComparerFixture : CoreTest
    {
        private List<Series> _series;

        [SetUp]
        public void Setup()
        {
            _series = new List<Series>();
        }

        private void WithSeries(string title)
        {
            _series.Add(new Series { Title = title });
        }

        [Test]
        public void should_prefer_the_walking_dead_over_talking_dead_when_searching_for_the_walking_dead()
        {
            WithSeries("Talking Dead");
            WithSeries("The Walking Dead");

            _series.Sort(new SearchSeriesComparer("the walking dead"));

            _series.First().Title.Should().Be("The Walking Dead");
        }

        [Test]
        public void should_prefer_the_walking_dead_over_talking_dead_when_searching_for_walking_dead()
        {
            WithSeries("Talking Dead");
            WithSeries("The Walking Dead");

            _series.Sort(new SearchSeriesComparer("walking dead"));

            _series.First().Title.Should().Be("The Walking Dead");
        }

        [Test]
        public void should_prefer_blacklist_over_the_blacklist_when_searching_for_blacklist()
        {
            WithSeries("The Blacklist");
            WithSeries("Blacklist");

            _series.Sort(new SearchSeriesComparer("blacklist"));

            _series.First().Title.Should().Be("Blacklist");
        }

        [Test]
        public void should_prefer_the_blacklist_over_blacklist_when_searching_for_the_blacklist()
        {
            WithSeries("Blacklist");
            WithSeries("The Blacklist");

            _series.Sort(new SearchSeriesComparer("the blacklist"));

            _series.First().Title.Should().Be("The Blacklist");
        }
    }
}
