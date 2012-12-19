// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using NUnit.Framework;
using Ninject;
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
    public class ParseDayOfWeekFixture : CoreTest
    {
        [Test]
        public void should_return_null_if_xelement_is_null()
        {
            Mocker.Resolve<TvRageProvider>().ParseDayOfWeek(null).Should().Be(null);
        }

        [Test]
        public void should_return_null_if_value_is_null()
        {
            Mocker.Resolve<TvRageProvider>().ParseDayOfWeek(new XElement("airday", null)).Should().Be(null);
        }

        [Test]
        public void should_return_null_if_value_is_empty()
        {
            Mocker.Resolve<TvRageProvider>().ParseDayOfWeek(new XElement("airday", "")).Should().Be(null);
        }

        [Test]
        public void should_return_null_if_value_is_daily()
        {
            Mocker.Resolve<TvRageProvider>().ParseDayOfWeek(new XElement("airday", "Daily")).Should().Be(null);
        }

        [Test]
        public void should_return_null_if_value_is_weekdays()
        {
            Mocker.Resolve<TvRageProvider>().ParseDayOfWeek(new XElement("airday", "Weekdays")).Should().Be(null);
        }

        [TestCase("Sunday", DayOfWeek.Sunday)]
        [TestCase("Monday", DayOfWeek.Monday)]
        [TestCase("Tuesday", DayOfWeek.Tuesday)]
        [TestCase("Wednesday", DayOfWeek.Wednesday)]
        [TestCase("Thursday", DayOfWeek.Thursday)]
        [TestCase("Friday", DayOfWeek.Friday)]
        [TestCase("Saturday", DayOfWeek.Saturday)]
        public void should_return_dayOfWeek_when_it_is_valid(string value, DayOfWeek expected)
        {
            Mocker.Resolve<TvRageProvider>().ParseDayOfWeek(new XElement("airday", value)).Should().Be(expected);
        }
    }
}