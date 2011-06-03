using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class SceneNameHelperTest : TestBase
    {

        [Test]
        public  void GetIdByName_exists()
        {
            var id = SceneNameHelper.GetIdByName("CSI New York");
            id.Should().Be(73696);
        }


        [Test]
        public  void GetTitleById_exists()
        {
            var title = SceneNameHelper.GetTitleById(71256);
            title.Should().Be("The Daily Show");
        }
    }
}
