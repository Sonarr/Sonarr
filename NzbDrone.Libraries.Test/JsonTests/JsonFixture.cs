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
            public int Id { get; set; }
        }

        [Test]
        public void should_be_able_to_deserialize_numbers()
        {
            var quality = new TypeWithNumbers { Id = 12 };

            Json.Deserialize<TypeWithNumbers>(quality.ToJson());
        }
    }
}
