using System.IO;
using NUnit.Framework;
using NzbDrone.Api.Frontend;
using NzbDrone.Test.Common;

namespace NzbDrone.Api.Test
{
    [TestFixture]
    public class StaticResourceMapperFixture : TestBase<StaticResourceMapper>
    {
        [TestCase("/app.js", Result = "ui|app.js")]
        [TestCase("/series/app.js", Result = "ui|series|app.js")]
        [TestCase("series/app.js", Result = "ui|series|app.js")]
        [TestCase("Series/App.js", Result = "ui|series|app.js")]
        public string should_map_paths(string path)
        {
            return Subject.Map(path).Replace(Path.DirectorySeparatorChar, '|');
        }

    }
}
