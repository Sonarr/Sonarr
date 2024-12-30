using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using Workarr.ThingiProvider;

namespace NzbDrone.Core.Test.ThingiProviderTests
{
    [TestFixture]
    public class NullConfigFixture : CoreTest<NullConfig>
    {
        [Test]
        public void should_be_valid()
        {
            Subject.Validate().IsValid.Should().BeTrue();
        }
    }
}
