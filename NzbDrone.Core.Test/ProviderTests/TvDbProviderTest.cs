// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Ninject;
using NzbDrone.Common;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using TvdbLib.Data;
using TvdbLib.Exceptions;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class TvDbProviderTest : CoreTest
    {
        private TvDbProvider tvDbProvider;

        [SetUp]
        public void Setup()
        {
            tvDbProvider = new StandardKernel().Get<TvDbProvider>();
        }

        [TearDown]
        public void TearDown()
        {
            ExceptionVerification.MarkInconclusive(typeof(TvdbNotAvailableException));
        }

        [TestCase("The Simpsons")]
        [TestCase("Family Guy")]
        [TestCase("South Park")]
        [TestCase("Franklin & Bash")]
        public void successful_search(string title)
        {
            var result = tvDbProvider.SearchSeries(title);

            result.Should().NotBeEmpty();
            result[0].SeriesName.Should().Be(title);
        }


        [Test]
        public void no_search_result()
        {
            //act
            var result = tvDbProvider.SearchSeries(Guid.NewGuid().ToString());

            //assert
            result.Should().BeEmpty();
        }


        [Test]
        public void none_unique_season_episode_number()
        {
            //act
            var result = tvDbProvider.GetSeries(75978, true);//Family guy

            //Asserts that when episodes are grouped by Season/Episode each group contains maximum of
            //one item.
            result.Episodes.GroupBy(e => e.SeasonNumber.ToString("000") + e.EpisodeNumber.ToString("000"))
                .Max(e => e.Count()).Should().Be(1);

        }
    }
}