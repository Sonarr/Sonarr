using System;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using NzbDrone.Core.Download.Clients.Sabnzbd;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.SabnzbdTests.JsonConvertersTests
{
    [TestFixture]
    public class SabnzbdQueueTimeConverterFixture
    {
        private const string QUERY = "{{ timeleft : '{0}' }}";

        [TestCase("0:0:0", 0)]
        [TestCase("0:1:59", 119)]
        [TestCase("0:59:59", 3599)]
        [TestCase("1:0:0", 3600)]
        [TestCase("1:0:0:1", (24 * 3600) + 1)]
        [TestCase("40:12:14", (40 * 3600) + (12 * 60) + 14)]
        [TestCase("1:16:12:14", (40 * 3600) + (12 * 60) + 14)]
        public void valid_time_formats_should_be_parsed_correctly(string time, int expectedSeconds)
        {
            var thing = string.Format(QUERY, time);
            var item = JsonConvert.DeserializeObject<SabnzbdQueueItem>(thing);
            item.Timeleft.Should().Be(TimeSpan.FromSeconds(expectedSeconds));
        }

        [TestCase("1")]
        [TestCase("0:1")]
        [TestCase("0:0:0:0:1")]
        public void invalid_time_formats_should_throw_an_exception(string time)
        {
            var thing = string.Format(QUERY, time);
            Assert.That(() => JsonConvert.DeserializeObject<SabnzbdQueueItem>(thing), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void should_support_pre_1_1_0rc4_format()
        {
            var thing = string.Format(QUERY, "40:12:14");
            var item = JsonConvert.DeserializeObject<SabnzbdQueueItem>(thing);
            item.Timeleft.Should().Be(new TimeSpan(40, 12, 14));
        }
    }
}
