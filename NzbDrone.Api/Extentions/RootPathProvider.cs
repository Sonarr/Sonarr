using System.IO;
using System.Linq;
using Nancy;

namespace NzbDrone.Api.Extentions
{
    public class RootPathProvider : IRootPathProvider
    {
        public string GetRootPath()
        {
            return Directory.GetCurrentDirectory();
        }
    }
}