using System;
using System.IO;

namespace NzbDrone.Core.Providers.Core
{
    public interface IDiskProvider
    {
        bool FolderExists(string path);
        string[] GetDirectories(string path);
        String CreateDirectory(string path);
        string[] GetFiles(string path, string pattern, SearchOption searchOption);
        bool FileExists(string path);
        long GetSize(string path);
        void DeleteFile(string path);
        void RenameFile(string sourcePath, string destinationPath);
    }
}