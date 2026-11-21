using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.ImportLists.MDBList;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests.MDBList
{
    [TestFixture]
    public class MDBListSettingsFixture : CoreTest
    {
        [Test]
        public void should_fail_validation_without_sign_in()
        {
            var settings = new MDBListSettings { ListId = "https://mdblist.com/lists/username/list-name" };

            settings.Validate().IsValid.Should().BeFalse();
        }

        [Test]
        public void should_pass_validation_when_signed_in()
        {
            var settings = new MDBListSettings { ListId = "https://mdblist.com/lists/username/list-name", AccessToken = "token" };

            settings.Validate().IsValid.Should().BeTrue();
        }
    }
}
