using System.Collections.Generic;
using Dapper;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Datastore
{
    [TestFixture]
    public class TableMapperFixture
    {
        public class EmbeddedType : IEmbeddedDocument
        {
        }

        public class TypeWithAllMappableProperties
        {
            public string PropString { get; set; }
            public int PropInt { get; set; }
            public bool PropBool { get; set; }
            public int? PropNullable { get; set; }
            public EmbeddedType Embedded { get; set; }
            public List<EmbeddedType> EmbeddedList { get; set; }
        }

        public class TypeWithNoMappableProperties
        {
            public Series Series { get; set; }

            public int ReadOnly { get; private set; }
            public int WriteOnly { private get; set; }
        }

        [SetUp]
        public void Setup()
        {
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<EmbeddedType>>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<EmbeddedType>());
        }

        [Test]
        public void test_mappable_types()
        {
            var properties = typeof(TypeWithAllMappableProperties).GetProperties();
            properties.Should().NotBeEmpty();
            properties.Should().OnlyContain(c => c.IsMappableProperty());
        }

        [Test]
        public void test_un_mappable_types()
        {
            var properties = typeof(TypeWithNoMappableProperties).GetProperties();
            properties.Should().NotBeEmpty();
            properties.Should().NotContain(c => c.IsMappableProperty());
        }
    }
}
