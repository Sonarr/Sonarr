using System.Collections.Generic;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.EpisodeImport.Manual;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Manual
{
    [TestFixture]
    public class ManualImportServiceFixture : CoreTest<ManualImportService>
    {
        [Test]
        public void should_not_throw_if_files_is_null()
        {
            Assert.DoesNotThrow(() => Subject.Execute(new ManualImportCommand { Files = null }));
        }

        [Test]
        public void should_not_throw_if_files_is_empty()
        {
            Assert.DoesNotThrow(() => Subject.Execute(new ManualImportCommand { Files = new List<ManualImportFile>() }));
        }
    }
}
