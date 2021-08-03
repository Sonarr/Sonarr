using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Core.Test.DataAugmentation.Scene
{
    [TestFixture]
    [IntegrationTest]
    public class SceneMappingProxyFixture : CoreTest<SceneMappingProxy>
    {
        [SetUp]
        public void Setup()
        {
            UseRealHttp();
        }

        [Test]
        public void fetch_should_return_list_of_mappings()
        {
            var mappings = Subject.Fetch();

            mappings.Should().NotBeEmpty();

            mappings.Should().NotContain(c => c.SearchTerm.IsNullOrWhiteSpace());
            mappings.Should().NotContain(c => c.Title.IsNullOrWhiteSpace());
            mappings.Should().Contain(c => c.SeasonNumber > 0);
        }
    }
}
