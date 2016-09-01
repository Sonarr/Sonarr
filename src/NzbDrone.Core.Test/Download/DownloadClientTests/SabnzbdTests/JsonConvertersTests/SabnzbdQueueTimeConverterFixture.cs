using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using NzbDrone.Core.Download.Clients.Sabnzbd;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.SabnzbdTests.JsonConvertersTests
{
    [TestFixture]
    public class SabnzbdQueueTimeConverterFixture
    {
        private const string QUERY =
            "{0} status : 'Downloading', mb : 1000, filename : 'Droned.S01E01.Pilot.1080p.WEB-DL-DRONE', priority : 0, cat : 'tv', mbleft : 10, percentage : 90, nzo_id : 'sabnzbd_nzb12345', timeleft : '{1}' {2}";

        [TestCase("0:0:1")]
        [TestCase("0:0:0:1")]
        public void valid_time_formats_should_be_parsed_correctly(string time)
        {
            var thing = string.Format(QUERY, "{", time, "}");
            var item = JsonConvert.DeserializeObject<SabnzbdQueueItem>(thing);
            item.Timeleft.Should().Be(new TimeSpan(0, 0, 1));
        }

        [TestCase("0:1")]
        [TestCase("1")]
        [TestCase("0:0:0:0:1")]
        public void invalid_time_formats_should_throw_an_exception(string time)
        {
            var thing = string.Format(QUERY, "{", time, "}");
            Assert.That(() => JsonConvert.DeserializeObject<SabnzbdQueueItem>(thing), Throws.TypeOf<ArgumentException>());
        }

        [TestCase("40:12:14")]
        [TestCase("1:16:12:14")]
        public void valid_time_formats_of_equal_value_should_be_parsed_the_same(string time)
        {
            var thing = string.Format(QUERY, "{", time, "}");
            var item = JsonConvert.DeserializeObject<SabnzbdQueueItem>(thing);
            item.Timeleft.Should().Be(new TimeSpan(40, 12, 14));
        }
    }
}
