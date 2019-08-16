using System;
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

        [Test]
        public void should_log_start_snippet_on_failure()
        {
            try
            {
                Json.Deserialize<object>("asdfl kasjd fsdfs derers");
            }
            catch (Exception ex)
            {
                ex.Message.Should().Contain("snippet '<--error-->asdfl kasjd fsdfs de'");
            }
        }

        [Test]
        public void should_log_line_snippet_on_failure()
        {
            try
            {
                Json.Deserialize<object>("{ \"a\": \r\n\"b\",\r\n \"b\": \"c\", asdfl kasjd fsdfs derers vsdfsdf");
            }
            catch (Exception ex)
            {
                ex.Message.Should().Contain("snippet ' \"b\": \"c\", asdfl <--error-->kasjd fsdfs derers v'");
            }
        }
    }
}
