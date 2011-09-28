using System;
using System.IO;
using System.Net;
using System.ServiceModel.Syndication;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class FluentTest : TestBase
    {
        [TestCase(null, "def", "def")]
        [TestCase("", "def", "def")]
        [TestCase("", 1, "1")]
        [TestCase(null, "", "")]
        [TestCase("actual", "def", "actual")]
        public void WithDefault_success(string actual, object defaultValue, string result)
        {
            actual.WithDefault(defaultValue).Should().Be(result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WithDefault_Fail()
        {
            "test".WithDefault(null);
        }

        [Test]
        public void ToBestDateTime_Yesterday()
        {
            var dateTime = DateTime.Today.AddDays(-1);

            dateTime.ToBestDateString().Should().Be("Yesterday");
        }

        [Test]
        public void ToBestDateTime_Today()
        {
            var dateTime = DateTime.Today;

            dateTime.ToBestDateString().Should().Be("Today");
        }

        [Test]
        public void ToBestDateTime_Tomorrow()
        {
            var dateTime = DateTime.Today.AddDays(1);

            dateTime.ToBestDateString().Should().Be("Tomorrow");
        }

        [Test]
        public void ToBestDateTime_DayOfWeek()
        {
            for (int i = 2; i < 7; i++)
            {
                var dateTime = DateTime.Today.AddDays(i);

                Console.WriteLine(dateTime.DayOfWeek);
                dateTime.ToBestDateString().Should().Be(dateTime.DayOfWeek.ToString());
            }
        }

        [Test]
        public void ToBestDateTime_Over_One_Week()
        {
            var dateTime = DateTime.Today.AddDays(8);

            Console.WriteLine(dateTime.DayOfWeek);
            dateTime.ToBestDateString().Should().Be(dateTime.ToShortDateString());
        }

        [Test]
        public void ToBestDateTime_Before_Yesterday()
        {
            var dateTime = DateTime.Today.AddDays(-2);

            Console.WriteLine(dateTime.DayOfWeek);
            dateTime.ToBestDateString().Should().Be(dateTime.ToShortDateString());
        }
    }
}
