// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class TvDbProviderTest : CoreTest
    {
        private TvDbProxy tvDbProxy;

        [SetUp]
        public void Setup()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<EnvironmentProvider>();
            builder.RegisterType<TvDbProxy>();

            var container = builder.Build();

            tvDbProxy = container.Resolve<TvDbProxy>();
        }

        [TearDown]
        public void TearDown()
        {
            //Todo: Is there a similar exception for wattvdb?
            //ExceptionVerification.MarkInconclusive(typeof(TvdbNotAvailableException));
        }

        [TestCase("The Simpsons")]
        [TestCase("Family Guy")]
        [TestCase("South Park")]
        [TestCase("Franklin & Bash")]
        public void successful_search(string title)
        {
            var result = tvDbProxy.SearchSeries(title);

            result.Should().NotBeEmpty();
            result[0].Title.Should().Be(title);
        }


        [Test]
        public void no_search_result()
        {
            //act
            var result = tvDbProxy.SearchSeries(Guid.NewGuid().ToString());

            //assert
            result.Should().BeEmpty();
        }


        [Test]
        public void none_unique_season_episode_number()
        {
            //act
            var result = tvDbProxy.GetEpisodes(75978);//Family guy

            //Asserts that when episodes are grouped by Season/Episode each group contains maximum of
            //one item.
            result.GroupBy(e => e.SeasonNumber.ToString("000") + e.EpisodeNumber.ToString("000"))
                .Max(e => e.Count()).Should().Be(1);


            result.Select(c => c.TvDbEpisodeId).Should().OnlyHaveUniqueItems();

        }
    }
}