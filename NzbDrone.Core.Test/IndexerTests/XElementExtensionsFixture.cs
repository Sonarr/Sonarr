using System;
using System.Xml.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Indexers;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.IndexerTests
{
    [TestFixture]
    public class XElementExtensionsFixture : TestBase
    {
        [TestCase("Wed, 07 Aug 2013 20:37:48 +0000")]
        [TestCase("Wed, 07 Aug 2013 18:08:46 MST")]
        public void should_parse_date(string dateString)
        {
            var element = new XElement("root");
            element.Add(new XElement("pubDate", dateString));

            var date = element.PublishDate();
            date.Year.Should().Be(2013);
            date.Kind.Should().Be(DateTimeKind.Utc);
        }
    }
}
