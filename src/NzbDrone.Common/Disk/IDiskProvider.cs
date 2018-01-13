using System;
using System.Collections.Generic;
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
        DateTime FolderGetCreationTime(string path);
        DateTime FolderGetLastWrite(string path);
        DateTime FileGetLastWrite(string path);
        void EnsureFolder(string path);
        bool FolderExists(string path);
        bool FileExists(string path);
        bool FileExists(string path, StringComparison stringComparison);
        bool FolderWritable(string path);
        string[] GetDirectories(string path);
        string[] GetFiles(string path, SearchOption searchOption);
        long GetFolderSize(string path);
        long GetFileSize(string path);
        void CreateFolder(string path);
        void DeleteFile(string path);
        void CopyFile(string source, string destination, bool overwrite = false);
        void MoveFile(string source, string destination, bool overwrite = false);
        bool TryCreateHardLink(string source, string destination);
        void DeleteFolder(string path, bool recursive);
        string ReadAllText(string filePath);
        void WriteAllText(string filename, string contents);
        void FolderSetLastWriteTime(string path, DateTime dateTime);
        void FileSetLastWriteTime(string path, DateTime dateTime);
        bool IsFileLocked(string path);
        string GetPathRoot(string path);
        string GetParentFolder(string path);
        void SetPermissions(string filename, WellKnownSidType accountSid, FileSystemRights rights, AccessControlType controlType);
        FileAttributes GetFileAttributes(string path);
        void EmptyFolder(string path);
        string GetVolumeLabel(string path);
        FileStream OpenReadStream(string path);
        FileStream OpenWriteStream(string path);
        List<IMount> GetMounts();
        IMount GetMount(string path);
        List<DirectoryInfo> GetDirectoryInfos(string path);
        List<FileInfo> GetFileInfos(string path);
        void RemoveEmptySubfolders(string path);
        void SaveStream(Stream stream, string path);
    }
}
