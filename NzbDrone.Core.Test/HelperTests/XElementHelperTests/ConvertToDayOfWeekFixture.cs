// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using NUnit.Framework;
using Ninject;
using NzbDrone.Common;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using TvdbLib.Data;
using TvdbLib.Exceptions;

namespace NzbDrone.Core.Test.HelperTests.XElementHelperTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class ParseDayOfWeekFixture : CoreTest
    {
        [Test]
        public void should_return_null_if_xelement_is_null()
        {
            XElement test = null;
            test.ConvertToDayOfWeek().Should().Be(null);
        }

        [Test]
        public void should_return_null_if_value_is_null()
        {
            new XElement("airday", null).ConvertToDayOfWeek().Should().Be(null);
        }

        [Test]
        public void should_return_null_if_value_is_empty()
        {
            new XElement("airday", "").ConvertToDayOfWeek().Should().Be(null);
        }

        [Test]
        public void should_return_null_if_value_is_daily()
        {
            new XElement("airday", "Daily").ConvertToDayOfWeek().Should().Be(null);
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
            new XElement("airday", value).ConvertToDayOfWeek().Should().Be(expected);
        }
    }
}