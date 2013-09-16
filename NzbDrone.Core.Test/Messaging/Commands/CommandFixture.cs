using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Update.Commands;

namespace NzbDrone.Core.Test.Messaging.Commands
{
    [TestFixture]
    public class CommandFixture
    {
        [Test]
        public void default_values()
        {
            var command = new ApplicationUpdateCommand();

            command.Id.Should().NotBe(0);
            command.Name.Should().Be("ApplicationUpdate");
        }
    }
}