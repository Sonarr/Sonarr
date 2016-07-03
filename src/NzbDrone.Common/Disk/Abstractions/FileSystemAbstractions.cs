using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NzbDrone.Common.Disk.Abstractions {

    public interface IFileSystemInfo {
        FileAttributes Attributes { get; }
        DateTimeOffset CreationTime { get; }
        DateTimeOffset CreationTimeUtc { get; }
        bool Exists { get; }
        string Extension { get; }
        string FullName { get; }
        DateTimeOffset LastAccessTime { get; }
        DateTimeOffset LastAccessTimeUtc { get; }
        DateTimeOffset LastWriteTime { get; }
        DateTimeOffset LastWriteTimeUtc { get; }
        string Name { get; }
        string LogicalName { get; }
        long? CalculateSize();
    }
    public interface IFileInfo : IFileSystemInfo {
        IDirectoryInfo Directory { get; }
        string DirectoryName { get; }
        bool IsReadOnly { get; }
        long Length { get; }
    }
    public interface IDirectoryInfo : IFileSystemInfo {
        IDirectoryInfo Parent { get; }
        IDirectoryInfo Root { get; }

        IEnumerable<IDirectoryInfo> EnumerateDirectories();
        IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern);
        IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption);
        IEnumerable<IFileInfo> EnumerateFiles();
        IEnumerable<IFileInfo> EnumerateFiles(string searchPattern);
        IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption);
        IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos();
        IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern);
        IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern, SearchOption searchOption);
        IDirectoryInfo[] GetDirectories();
        IDirectoryInfo[] GetDirectories(string searchPattern);
        IDirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption);
        IFileInfo[] GetFiles();
        IFileInfo[] GetFiles(string searchPattern);
        IFileInfo[] GetFiles(string searchPattern, SearchOption searchOption);
        IFileSystemInfo[] GetFileSystemInfos();
        IFileSystemInfo[] GetFileSystemInfos(string searchPattern);
        IFileSystemInfo[] GetFileSystemInfos(string searchPattern, SearchOption searchOption);
    }



    [DebuggerDisplay("{FullName}")]
    internal abstract class FileSystemInfoWrapper<T> : IFileSystemInfo
       where T : FileSystemInfo {

        protected T FileSystemInfo { get; private set; }

        protected FileSystemInfoWrapper(T fileSystemInfo) {
            if (fileSystemInfo == null)
                throw new ArgumentNullException("fileSystemInfo");

            this.FileSystemInfo = fileSystemInfo;
        }
        public FileAttributes Attributes
        {
            get
            {
                return this.FileSystemInfo.Attributes;
            }
        }

        public DateTimeOffset CreationTime
        {
            get
            {
                return new DateTimeOffset(this.FileSystemInfo.CreationTime);
            }
        }

        public DateTimeOffset CreationTimeUtc
        {
            get
            {
                return new DateTimeOffset(this.FileSystemInfo.CreationTimeUtc, TimeSpan.Zero);
            }
        }

        public bool Exists
        {
            get
            {
                return this.FileSystemInfo.Exists;
            }
        }

        public string Extension
        {
            get
            {
                return this.FileSystemInfo.Extension;
            }
        }

        public string FullName
        {
            get
            {
                return this.FileSystemInfo.FullName;
            }
        }

        public DateTimeOffset LastAccessTime
        {
            get
            {
                return new DateTimeOffset(this.FileSystemInfo.LastAccessTime);
            }

        }

        public DateTimeOffset LastAccessTimeUtc
        {
            get
            {
                return new DateTimeOffset(this.FileSystemInfo.LastAccessTimeUtc, TimeSpan.Zero);
            }
        }

        public DateTimeOffset LastWriteTime
        {
            get
            {
                return new DateTimeOffset(this.FileSystemInfo.LastWriteTime);
            }
        }

        public DateTimeOffset LastWriteTimeUtc
        {
            get
            {
                return new DateTimeOffset(this.FileSystemInfo.LastWriteTimeUtc, TimeSpan.Zero);
            }
        }

        public string Name
        {
            get
            {
                return this.FileSystemInfo.Name;
            }
        }

        public abstract string LogicalName { get; }

        public abstract long? CalculateSize();
    }
    internal class FileInfoWrapper : FileSystemInfoWrapper<FileInfo>, IFileInfo {

        public FileInfoWrapper(FileInfo fileSystemInfo) : base(fileSystemInfo) {
        }

        public IDirectoryInfo Directory
        {
            get
            {
                return FileSystemInfoFactory.CreateFrom(this.FileSystemInfo.Directory);
            }
        }

        public string DirectoryName
        {
            get
            {
                return this.FileSystemInfo.DirectoryName;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.FileSystemInfo.IsReadOnly;
            }
        }

        public long Length
        {
            get
            {
                return this.FileSystemInfo.Length;
            }
        }

        public override string LogicalName
        {
            get
            {
                return Path.GetFileNameWithoutExtension(this.Name);
            }
        }

        public override long? CalculateSize() {
            if (!this.FileSystemInfo.Exists)
                return null;

            return this.FileSystemInfo.Length;
        }
    }
    internal class DirectoryInfoWrapper : FileSystemInfoWrapper<DirectoryInfo>, IDirectoryInfo {

        public DirectoryInfoWrapper(DirectoryInfo fileSystemInfo) : base(fileSystemInfo) {
        }

        public IDirectoryInfo Parent
        {
            get
            {
                return FileSystemInfoFactory.CreateFrom(this.FileSystemInfo.Parent);
            }
        }

        public IDirectoryInfo Root
        {
            get
            {
                return FileSystemInfoFactory.CreateFrom(this.FileSystemInfo.Root);
            }
        }

        public IEnumerable<IDirectoryInfo> EnumerateDirectories() {
            return this.FileSystemInfo.EnumerateDirectories().Select(FileSystemInfoFactory.CreateFrom);
        }

        public IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern) {
            return this.FileSystemInfo.EnumerateDirectories(searchPattern).Select(FileSystemInfoFactory.CreateFrom);
        }

        public IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption) {
            return this.FileSystemInfo.EnumerateDirectories(searchPattern, searchOption).Select(FileSystemInfoFactory.CreateFrom);
        }

        public IEnumerable<IFileInfo> EnumerateFiles() {
            return this.FileSystemInfo.EnumerateFiles().Select(FileSystemInfoFactory.CreateFrom);
        }

        public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern) {
            return this.FileSystemInfo.EnumerateFiles(searchPattern).Select(FileSystemInfoFactory.CreateFrom);
        }

        public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption) {
            return this.FileSystemInfo.EnumerateFiles(searchPattern, searchOption).Select(FileSystemInfoFactory.CreateFrom);
        }

        public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos() {
            return this.FileSystemInfo.EnumerateFileSystemInfos().Select(FileSystemInfoFactory.CreateFrom);
        }

        public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern) {
            return this.FileSystemInfo.EnumerateFileSystemInfos(searchPattern).Select(FileSystemInfoFactory.CreateFrom);
        }

        public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern, SearchOption searchOption) {
            return this.FileSystemInfo.EnumerateFileSystemInfos(searchPattern, searchOption).Select(FileSystemInfoFactory.CreateFrom);
        }

        public IDirectoryInfo[] GetDirectories() {
            return this.FileSystemInfo.GetDirectories().Select(FileSystemInfoFactory.CreateFrom).ToArray();
        }

        public IDirectoryInfo[] GetDirectories(string searchPattern) {
            return this.FileSystemInfo.GetDirectories(searchPattern).Select(FileSystemInfoFactory.CreateFrom).ToArray();
        }

        public IDirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption) {
            return this.FileSystemInfo.GetDirectories(searchPattern, searchOption).Select(FileSystemInfoFactory.CreateFrom).ToArray();
        }

        public IFileInfo[] GetFiles() {
            return this.FileSystemInfo.GetFiles().Select(FileSystemInfoFactory.CreateFrom).ToArray();
        }

        public IFileInfo[] GetFiles(string searchPattern) {
            return this.FileSystemInfo.GetFiles(searchPattern).Select(FileSystemInfoFactory.CreateFrom).ToArray();
        }

        public IFileInfo[] GetFiles(string searchPattern, SearchOption searchOption) {
            return this.FileSystemInfo.GetFiles(searchPattern, searchOption).Select(FileSystemInfoFactory.CreateFrom).ToArray();
        }

        public IFileSystemInfo[] GetFileSystemInfos() {
            return this.FileSystemInfo.GetFileSystemInfos().Select(FileSystemInfoFactory.CreateFrom).ToArray();
        }

        public IFileSystemInfo[] GetFileSystemInfos(string searchPattern) {
            return this.FileSystemInfo.GetFileSystemInfos(searchPattern).Select(FileSystemInfoFactory.CreateFrom).ToArray();
        }

        public IFileSystemInfo[] GetFileSystemInfos(string searchPattern, SearchOption searchOption) {
            return this.FileSystemInfo.GetFileSystemInfos(searchPattern, searchOption).Select(FileSystemInfoFactory.CreateFrom).ToArray();
        }

        public override long? CalculateSize() {
            if (!this.FileSystemInfo.Exists)
                return null;

            return this.FileSystemInfo.EnumerateFiles("*", SearchOption.AllDirectories)
                .Sum(x => x.Length);
        }

        public override string LogicalName
        {
            get
            {
                return this.Name;
            }
        }
    }
}
