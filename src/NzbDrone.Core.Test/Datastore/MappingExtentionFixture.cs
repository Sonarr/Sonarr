using System.Collections.Generic;
using FluentAssertions;
using Marr.Data;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Datastore.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Datastore
{
    [TestFixture]
    public class MappingExtensionFixture
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
            MapRepository.Instance.RegisterTypeConverter(typeof(List<EmbeddedType>), new EmbeddedDocumentConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(EmbeddedType), new EmbeddedDocumentConverter());
            MapRepository.Instance.RegisterTypeConverter(typeof(int), new Int32Converter());
        }

        [Test]
        public void test_mappable_types()
        {
            var properties = typeof(TypeWithAllMappableProperties).GetProperties();
            properties.Should().NotBeEmpty();
            properties.Should().OnlyContain(c => MappingExtensions.IsMappableProperty(c));
        }

        [Test]
        public void test_un_mappable_types()
        {
            var properties = typeof(TypeWithNoMappableProperties).GetProperties();
            properties.Should().NotBeEmpty();
            properties.Should().NotContain(c => MappingExtensions.IsMappableProperty(c));
        }
    }
}
