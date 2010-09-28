using System;

namespace NzbDrone.Core.Providers
{
    public interface IDiskProvider
    {
        bool Exists(string path);
        string[] GetDirectories(string path);
        String CreateDirectory(string path);
    }
}