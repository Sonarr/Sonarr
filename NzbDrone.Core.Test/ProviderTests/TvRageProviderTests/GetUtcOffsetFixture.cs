// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using TvdbLib.Data;
using TvdbLib.Exceptions;

namespace NzbDrone.Core.Test.ProviderTests.TvRageProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class GetUtcOffsetFixture : SqlCeTest
    {
        [Test]
        public void should_return_zero_if_timeZone_is_empty()
        {
            Mocker.Resolve<TvRageProvider>().GetUtcOffset("").Should().Be(0);
        }

        [Test]
        public void should_return_zero_if_cannot_be_coverted_to_int()
        {
            Mocker.Resolve<TvRageProvider>().GetUtcOffset("adfhadfhdjaf").Should().Be(0);
        }

        [TestCase("GMT-5", -5)]
        [TestCase("GMT+0", 0)]
        [TestCase("GMT+8", 8)]
        public void should_return_offset_when_not_dst(string timezone, int expected)
        {
            Mocker.Resolve<TvRageProvider>().GetUtcOffset(timezone).Should().Be(expected);
        }

        [TestCase("GMT-5 +DST", -4)]
        [TestCase("GMT+0 +DST", 1)]
        [TestCase("GMT+8 +DST", 9)]
        public void should_return_offset_plus_one_when_dst(string timezone, int expected)
        {
            Mocker.Resolve<TvRageProvider>().GetUtcOffset(timezone).Should().Be(expected);
        }
    }
}