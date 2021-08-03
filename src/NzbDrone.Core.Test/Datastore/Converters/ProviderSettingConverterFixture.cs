using System;
using FluentAssertions;
using Marr.Data.Converters;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Test.Datastore.Converters
{
    [TestFixture]
    public class ProviderSettingConverterFixture : CoreTest<ProviderSettingConverter>
    {
        [Test]
        public void should_return_null_config_if_config_is_null()
        {
            var result = Subject.FromDB(new ConverterContext()
             {
                 DbValue = DBNull.Value
             });

            result.Should().Be(NullConfig.Instance);
        }

        [TestCase(null)]
        [TestCase("")]
        public void should_return_null_config_if_config_is_empty(object dbValue)
        {
            var result = Subject.FromDB(new ConverterContext()
            {
                DbValue = dbValue
            });

            result.Should().Be(NullConfig.Instance);
        }
    }
}
