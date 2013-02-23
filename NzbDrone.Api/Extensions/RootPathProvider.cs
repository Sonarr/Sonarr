using System.IO;
using System.Linq;
using Nancy;

namespace NzbDrone.Api.Extensions
{
    public class RootPathProvider : IRootPathProvider
    {
        public string GetRootPath()
        {
            return Directory.GetCurrentDirectory();
        }
    }
}