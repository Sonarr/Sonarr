using System.Data.SQLite;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv.Commands;

namespace NzbDrone.Core.Test.Datastore.Converters
{
    [TestFixture]
    public class CommandConverterFixture : CoreTest<CommandConverter>
    {
        private SQLiteParameter _param;

        [SetUp]
        public void Setup()
        {
            _param = new SQLiteParameter();
        }

        [Test]
        public void should_return_json_string_when_saving_boolean_to_db()
        {
            var command = new RefreshSeriesCommand();

            Subject.SetValue(_param, command);
            _param.Value.Should().BeOfType<string>();
        }

        [Test]
        public void should_return_null_for_null_value_when_saving_to_db()
        {
            Subject.SetValue(_param, null);
            _param.Value.Should().BeNull();
        }

        [Test]
        public void should_return_command_when_getting_json_from_db()
        {
            var data = "{\"name\": \"RefreshSeries\"}";

            Subject.Parse(data).Should().BeOfType<RefreshSeriesCommand>();
        }

        [Test]
        public void should_return_unknown_command_when_getting_json_from_db()
        {
            var data = "{\"name\": \"EnsureMediaCovers\"}";

            Subject.Parse(data).Should().BeOfType<UnknownCommand>();
        }

        [Test]
        public void should_return_null_for_null_value_when_getting_from_db()
        {
            Subject.Parse(null).Should().BeNull();
        }
    }
}
