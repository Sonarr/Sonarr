using System;

namespace NzbDrone.Core.Controllers
{
    public interface IDiskController
    {
        bool Exists(string path);
        string[] GetDirectories(string path);
        String CreateDirectory(string path);
        string CleanPath(string path);
    }
}