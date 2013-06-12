using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Notifications.Xbmc;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NotificationTests.Xbmc.Json
{
    [TestFixture]
    public class CheckForErrorFixture : CoreTest<JsonApiProvider>
    {
        [Test]
        public void should_be_true_when_the_response_contains_an_error()
        {
            const string response = "{\"error\":{\"code\":-32601,\"message\":\"Method not found.\"},\"id\":10,\"jsonrpc\":\"2.0\"}";

            Subject.CheckForError(response).Should().BeTrue();
        }

        [Test]
        public void JsonError_true_empty_response()
        {
            var response = String.Empty;

            Subject.CheckForError(response).Should().BeTrue();
        }

        [Test]
        public void JsonError_false()
        {
            const string response = "{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"version\":3}}";

            Subject.CheckForError(response).Should().BeFalse();
        }
    }
}
