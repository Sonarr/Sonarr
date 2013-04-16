using NUnit.Framework;
using Newtonsoft.Json;

namespace NzbDrone.Libraries.Test.Json
{
    [TestFixture]
    public class JsonFixture
    {
        public class TypeWithNumbers
        {
            public int Id { get; set; }
        }

        [Test]
        public void should_be_able_to_deserialize_numbers()
        {
            var quality = new TypeWithNumbers { Id = 12 };

            var json = JsonConvert.SerializeObject(quality);

            JsonConvert.DeserializeObject(json);
        }
    }
}
