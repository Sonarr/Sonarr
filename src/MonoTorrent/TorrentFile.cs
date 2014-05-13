using System;
using System.Text;

namespace MonoTorrent
{
    /// <summary>
    /// This is the base class for the files available to download from within a .torrent.
    /// This should be inherited by both Client and Tracker "TorrentFile" classes
    /// </summary>
    public class TorrentFile : IEquatable<TorrentFile>
    {
        #region Private Fields

        private BitField bitfield;
        private BitField selector;
        private byte[] ed2k;
        private int endPiece;
        private string fullPath;
        private long length;
        private byte[] md5;
        private string path;
        private Priority priority;
        private byte[] sha1;
        private int startPiece;

        #endregion Private Fields


        #region Member Variables

        /// <summary>
        /// The number of pieces which have been successfully downloaded which are from this file
        /// </summary>
        public BitField BitField
        {
            get { return this.bitfield; }
        }

        public long BytesDownloaded
        {
            get { return (long)(this.BitField.PercentComplete * this.Length / 100.0); }
        }

        /// <summary>
        /// The ED2K hash of the file
        /// </summary>
        public byte[] ED2K
        {
            get { return this.ed2k; }
        }

        /// <summary>
        /// The index of the last piece of this file
        /// </summary>
        public int EndPieceIndex
        {
            get { return this.endPiece; }
        }

        public string FullPath
        {
            get { return this.fullPath; }
            internal set { this.fullPath = value; }
        }

        /// <summary>
        /// The length of the file in bytes
        /// </summary>
        public long Length
        {
            get { return this.length; }
        }

        /// <summary>
        /// The MD5 hash of the file
        /// </summary>
        public byte[] MD5
        {
            get { return this.md5; }
            internal set { this.md5 = value; }
        }

        /// <summary>
        /// In the case of a single torrent file, this is the name of the file.
        /// In the case of a multi-file torrent this is the relative path of the file
        /// (including the filename) from the base directory
        /// </summary>
        public string Path
        {
            get { return this.path; }
        }

        /// <summary>
        /// The priority of this torrent file
        /// </summary>
        public Priority Priority
        {
            get { return this.priority; }
            set { this.priority = value; }
        }

        /// <summary>
        /// The SHA1 hash of the file
        /// </summary>
        public byte[] SHA1
        {
            get { return this.sha1; }
        }

        /// <summary>
        /// The index of the first piece of this file
        /// </summary>
        public int StartPieceIndex
        {
            get { return this.startPiece; }
        }

        #endregion


        #region Constructors
        public TorrentFile(string path, long length)
            : this(path, length, path)
        {

        }

        public TorrentFile (string path, long length, string fullPath)
            : this (path, length, fullPath, 0, 0)
        {

        }

        public TorrentFile (string path, long length, int startIndex, int endIndex)
            : this (path, length, path, startIndex, endIndex)
        {

        }

        public TorrentFile(string path, long length, string fullPath, int startIndex, int endIndex)
            : this(path, length, fullPath, startIndex, endIndex, null, null, null)
        {

        }

        public TorrentFile(string path, long length, string fullPath, int startIndex, int endIndex, byte[] md5, byte[] ed2k, byte[] sha1)
        {
            this.bitfield = new BitField(endIndex - startIndex + 1);
            this.ed2k = ed2k;
            this.endPiece = endIndex;
            this.fullPath = fullPath;
            this.length = length;
            this.md5 = md5;
            this.path = path;
            this.priority = Priority.Normal;
            this.sha1 = sha1;
            this.startPiece = startIndex;
        }

        #endregion


        #region Methods

        public override bool Equals(object obj)
        {
            return this.Equals(obj as TorrentFile);
        }

        public bool Equals(TorrentFile other)
        {
            return other == null ? false : this.path == other.path && this.length == other.length; ;
        }

        public override int GetHashCode()
        {
            return this.path.GetHashCode();
        }

        internal BitField GetSelector(int totalPieces)
        {
            if (this.selector != null)
                return this.selector;

            this.selector = new BitField(totalPieces);
            for (int i = this.StartPieceIndex; i <= this.EndPieceIndex; i++)
                this.selector[i] = true;
            return this.selector;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(32);
            sb.Append("File: ");
            sb.Append(this.path);
            sb.Append(" StartIndex: ");
            sb.Append(this.StartPieceIndex);
            sb.Append(" EndIndex: ");
            sb.Append(this.EndPieceIndex);
            return sb.ToString();
        }

        #endregion Methods
    }
}