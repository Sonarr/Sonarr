using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Test.Common;

namespace NzbDrone.Libraries.Test.JsonTests
{
    [TestFixture]
    public class JsonFixture : TestBase
    {
        public class TypeWithNumbers
        {
            public int Int32 { get; set; }
            public long Int64 { get; set; }
            public int? nullableIntIsNull { get; set; }
            public int? nullableWithValue { get; set; }
        }

        [Test]
        public void should_be_able_to_deserialize_numbers()
        {
            var quality = new TypeWithNumbers { Int32 = int.MaxValue, Int64 = long.MaxValue, nullableWithValue = 12 };
            var result = Json.Deserialize<TypeWithNumbers>(quality.ToJson());

            result.Should().BeEquivalentTo(quality, o => o.IncludingAllRuntimeProperties());
        }
    }
}
