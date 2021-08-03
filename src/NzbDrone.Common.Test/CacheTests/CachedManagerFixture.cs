using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Cache;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.CacheTests
{
    [TestFixture]
    public class CachedManagerFixture : TestBase<ICacheManager>
    {
        [Test]
        public void should_return_proper_type_of_cache()
        {
            var result = Subject.GetCache<DateTime>(typeof(string));

            result.Should().BeOfType<Cached<DateTime>>();
        }

        [Test]
        public void multiple_calls_should_get_the_same_cache()
        {
            var result1 = Subject.GetCache<DateTime>(typeof(string));
            var result2 = Subject.GetCache<DateTime>(typeof(string));

            result1.Should().BeSameAs(result2);
        }
    }
}
