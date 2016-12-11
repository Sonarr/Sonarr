using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Api.Series;
using System.Linq;
using NzbDrone.Test.Common;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class EpisodeFileFixture : IntegrationTest
    {
        [Test]
        public void get_all_episodefiles()
        {
            Assert.Ignore("TODO");
        }
    }
}
