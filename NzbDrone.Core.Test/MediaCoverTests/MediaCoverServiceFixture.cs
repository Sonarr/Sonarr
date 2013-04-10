using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaCoverTests
{
    [TestFixture]
    public class MediaCoverServiceFixture : CoreTest<MediaCoverService>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.SetConstant(new HttpProvider(new EnvironmentProvider()));
            Mocker.SetConstant(new DiskProvider());
            Mocker.SetConstant(new EnvironmentProvider());
        }

    }
}