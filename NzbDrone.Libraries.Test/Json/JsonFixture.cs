using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Serializer;
using NzbDrone.Test.Common;

namespace NzbDrone.Libraries.Test.Json
{
    [TestFixture]
    public class JsonFixture : TestBase<JsonSerializer>
    {
        public class TypeWithNumbers
        {
            public int Id { get; set; }
        }

        [Test]
        public void should_be_able_to_deserialize_numbers()
        {
            var quality = new TypeWithNumbers { Id = 12 };

            var json = Subject.Serialize(quality);

            Subject.Deserialize<TypeWithNumbers>(json);
        }
    }
}
