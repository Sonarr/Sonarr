using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Test.ExtensionTests
{
    [TestFixture]
    public class Int64ExtensionFixture
    {
        [TestCase(1000, "1.0 KB")]
        [TestCase(1000000, "1.0 MB")]
        [TestCase(377487360, "377.5 MB")]
        [TestCase(1255864686, "1.3 GB")]
        public void should_calculate_string_correctly(long bytes, string expected)
        {
            bytes.SizeSuffix().Should().Be(expected);
        }
    }
}
