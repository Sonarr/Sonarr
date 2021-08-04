using System.Collections.Generic;
using System.Data.SQLite;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Converters
{
    [TestFixture]
    public class DictionaryConverterFixture : CoreTest<EmbeddedDocumentConverter<Dictionary<string, string>>>
    {
        private SQLiteParameter _param;

        [SetUp]
        public void Setup()
        {
            _param = new SQLiteParameter();
        }

        [Test]
        public void should_serialize_in_camel_case()
        {
            var dict = new Dictionary<string, string>
            {
                { "Data", "Should be lowercased" },
                { "CamelCase", "Should be cameled" }
            };

            Subject.SetValue(_param, dict);

            var result = (string)_param.Value;

            result.Should().Contain("data");
            result.Should().NotContain("Data");

            result.Should().Contain("camelCase");
            result.Should().NotContain("CamelCase");
        }
    }
}
