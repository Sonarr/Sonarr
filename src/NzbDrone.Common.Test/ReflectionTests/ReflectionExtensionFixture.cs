using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Test.Common;
using Workarr.Datastore;
using Workarr.Reflection;

namespace NzbDrone.Common.Test.ReflectionTests
{
    public class ReflectionExtensionFixture : TestBase
    {
        [Test]
        public void should_get_properties_from_models()
        {
            var models = Assembly.Load("Sonarr.Core").ImplementationsOf<ModelBase>();

            foreach (var model in models)
            {
                model.GetSimpleProperties().Should().NotBeEmpty();
            }
        }

        [Test]
        public void should_be_able_to_get_implementations()
        {
            var models = Assembly.Load("Sonarr.Core").ImplementationsOf<ModelBase>();

            models.Should().NotBeEmpty();
        }
    }
}
