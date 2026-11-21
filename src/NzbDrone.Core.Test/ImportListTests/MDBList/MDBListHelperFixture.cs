using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.ImportLists.MDBList;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests.MDBList
{
    [TestFixture]
    public class MDBListHelperFixture : CoreTest
    {
        [TestCase("https://mdblist.com/lists/username/list-name", "lists/username/list-name")]
        [TestCase("https://www.mdblist.com/lists/username/list-name/", "lists/username/list-name")]
        [TestCase("https://mdblist.com/lists/username/list-name?sort=score", "lists/username/list-name")]
        [TestCase("http://mdblist.com/lists/username/list-name", "lists/username/list-name")]
        [TestCase(" https://mdblist.com/lists/username/list-name ", "lists/username/list-name")]
        [TestCase("https://mdblist.com/lists/username/caf%C3%A9-shows", "lists/username/caf%C3%A9-shows")]
        public void should_accept_list_input(string list, string expectedPath)
        {
            MDBListHelper.GetListPath(list).Should().Be(expectedPath);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("12345")]
        [TestCase("username/list-name")]
        [TestCase("https://mdblist.com/lists/username")]
        [TestCase("https://mdblist.com/likedlists/username/list-name")]
        public void should_reject_list_input(string list)
        {
            MDBListHelper.GetListPath(list).Should().BeNull();
        }
    }
}
