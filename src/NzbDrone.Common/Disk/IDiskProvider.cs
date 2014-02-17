using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace NzbDrone.Common.Disk
{
    public interface IDiskProvider
    {
        long? GetAvailableSpace(string path);
        void InheritFolderPermissions(string filename);
        void SetPermissions(string path, string mask, string user, string group);
        long? GetTotalSize(string path);

        DateTime GetLastFolderWrite(string path);
        DateTime GetLastFileWrite(string path);
        void EnsureFolder(string path);
        bool FolderExists(string path);
        bool FileExists(string path);
        bool FileExists(string path, bool caseSensitive);
        string[] GetDirectories(string path);
        string[] GetFiles(string path, SearchOption searchOption);
        long GetFolderSize(string path);
        long GetFileSize(string path);
        void CreateFolder(string path);
        void CopyFolder(string source, string destination);
        void MoveFolder(string source, string destination);
        void DeleteFile(string path);
        void CopyFile(string source, string destination, bool overwrite = false);
        void MoveFile(string source, string destination);
        void DeleteFolder(string path, bool recursive);
        string ReadAllText(string filePath);
        void WriteAllText(string filename, string contents);
        void FileSetLastWriteTimeUtc(string path, DateTime dateTime);
        void FolderSetLastWriteTimeUtc(string path, DateTime dateTime);
        bool IsFileLocked(string path);
        string GetPathRoot(string path);
        string GetParentFolder(string path);
        void SetPermissions(string filename, WellKnownSidType accountSid, FileSystemRights rights, AccessControlType controlType);
        void SetFolderWriteTime(string path, DateTime time);
        FileAttributes GetFileAttributes(string path);
        void EmptyFolder(string path);
        string[] GetFixedDrives();
        string GetVolumeLabel(string path);
    }
}