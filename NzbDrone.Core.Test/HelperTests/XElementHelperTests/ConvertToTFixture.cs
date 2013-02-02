// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HelperTests.XElementHelperTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class XElementHelperTest : SqlCeTest
    {
        [Test]
        public void Int32_should_return_zero_when_xelement_is_null()
        {
            XElement test = null;

            test.ConvertTo<Int32>().Should().Be(0);
        }

        [Test]
        public void Int32_should_return_zero_when_value_is_null()
        {
            new XElement("test", null).ConvertTo<Int32>().Should().Be(0);
        }

        [Test]
        public void Int32_should_return_value_when_value_is_an_int()
        {
            new XElement("test", 10).ConvertTo<Int32>().Should().Be(10);
        }

        [Test]
        public void Nullable_Int32_should_return_null_when_xelement_is_null()
        {
            XElement test = null;

            test.ConvertTo<Nullable<Int32>>().Should().Be(null);
        }

        [Test]
        public void DateTime_should_return_zero_when_xelement_is_null()
        {
            XElement test = null;

            test.ConvertTo<DateTime>().Should().Be(DateTime.MinValue);
        }

        [Test]
        public void DateTime_should_return_zero_when_value_is_null()
        {
            new XElement("test", null).ConvertTo<DateTime>().Should().Be(DateTime.MinValue);
        }

        [Test]
        public void DateTime_should_return_value_when_value_is_a_date()
        {
            var date = DateTime.Today;
            new XElement("test", date.ToString()).ConvertTo<DateTime>().Should().Be(date);
        }
    }
}