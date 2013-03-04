using System.Linq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Test.MediaCoverTests
{
    [TestFixture]
    public class MediaCoverServiceFixture : CoreTest<MediaCoverService>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.SetConstant(new HttpProvider());
            Mocker.SetConstant(new DiskProvider());
            Mocker.SetConstant(new EnvironmentProvider());
        }

    }
}