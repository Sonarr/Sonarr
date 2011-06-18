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


    }
}
