using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.ImportLists.MDBList;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests.MDBList
{
    [TestFixture]
    public class MDBListSettingsFixture : CoreTest
    {
        [TestCase("12345", "lists/12345")]
        [TestCase(" 12345 ", "lists/12345")]
        [TestCase("https://mdblist.com/lists/username/list-name", "lists/username/list-name")]
        [TestCase("https://www.mdblist.com/lists/username/list-name/", "lists/username/list-name")]
        [TestCase("https://mdblist.com/lists/username/list-name?sort=score", "lists/username/list-name")]
        [TestCase("http://mdblist.com/lists/username/list-name", "lists/username/list-name")]
        [TestCase("username/list-name", "lists/username/list-name")]
        public void should_accept_list_input(string list, string expectedPath)
        {
            MDBListSettings.GetListPath(list).Should().Be(expectedPath);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("list-name")]
        [TestCase("https://example.com/lists/username/list-name")]
        [TestCase("https://mdblist.com/lists/username/list-name/extra")]
        public void should_reject_list_input(string list)
        {
            MDBListSettings.GetListPath(list).Should().BeNull();
        }

        [Test]
        public void should_fail_validation_without_sign_in()
        {
            var settings = new MDBListSettings { ListId = "12345" };

            settings.Validate().IsValid.Should().BeFalse();
        }

        [Test]
        public void should_pass_validation_when_signed_in()
        {
            var settings = new MDBListSettings { ListId = "12345", AccessToken = "token" };

            settings.Validate().IsValid.Should().BeTrue();
        }
    }
}
