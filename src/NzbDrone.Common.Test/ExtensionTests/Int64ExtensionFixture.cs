using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Test.ExtensionTests
{
    [TestFixture]
    public class Int64ExtensionFixture
    {
        [TestCase(0, "0 B")]
        [TestCase(1000, "1,000.0 B")]
        [TestCase(1024, "1.0 KB")]
        [TestCase(1000000, "976.6 KB")]
        [TestCase(377487360, "360.0 MB")]
        [TestCase(1255864686, "1.2 GB")]
        [TestCase(-1024, "-1.0 KB")]
        [TestCase(-1000000, "-976.6 KB")]
        [TestCase(-377487360, "-360.0 MB")]
        [TestCase(-1255864686, "-1.2 GB")]
        public void should_calculate_string_correctly(long bytes, string expected)
        {
            bytes.SizeSuffix().Should().Be(expected);
        }
    }
}
