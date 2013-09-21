using NUnit.Framework;
using NzbDrone.Core.Test.Datastore;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Test.ThingiProvider
{

    public class ProviderRepositoryFixture : DbTest<DownloadProviderRepository, DownloadProviderModel>
    {
        [Test]
        public void should_read_write_download_provider()
        {
            var model = new DownloadProviderModel();

            model.Config = new DownloadProviderConfig();

            //Subject.Insert(new )
        }


    }
}