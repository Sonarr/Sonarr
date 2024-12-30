using FluentAssertions;
using NUnit.Framework;
using Workarr.TPL;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class HashUtilFixture
    {
        [Test]
        public void should_create_anon_id()
        {
            HashUtil.AnonymousToken().Should().NotBeNullOrEmpty();
        }

        [Test]
        public void should_create_the_same_id()
        {
            HashUtil.AnonymousToken().Should().Be(HashUtil.AnonymousToken());
        }
    }
}
