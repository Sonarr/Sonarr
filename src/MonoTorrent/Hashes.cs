using System;

namespace MonoTorrent
{
    public class Hashes
    {
        #region Constants
        /// <summary>
        /// Hash code length (in bytes)
        /// </summary>
        internal static readonly int HashCodeLength = 20;
        #endregion


        #region Private Fields

        private int count;
        private byte[] hashData;

        #endregion Private Fields


        #region Properties

        /// <summary>
        /// Number of Hashes (equivalent to number of Pieces)
        /// </summary>
        public int Count
        {
            get { return this.count; }
        }

        #endregion Properties


        #region Constructors

        internal Hashes(byte[] hashData, int count)
        {
            this.hashData = hashData;
            this.count = count;
        }

        #endregion Constructors


        #region Methods

        /// <summary>
        /// Determine whether a calculated hash is equal to our stored hash
        /// </summary>
        /// <param name="hash">Hash code to check</param>
        /// <param name="hashIndex">Index of hash/piece to verify against</param>
        /// <returns>true iff hash is equal to our stored hash, false otherwise</returns>
        public bool IsValid(byte[] hash, int hashIndex)
        {
            if (hash == null)
                throw new ArgumentNullException("hash");

            if (hash.Length != HashCodeLength)
                throw new ArgumentException(string.Format("Hash must be {0} bytes in length", HashCodeLength), "hash");

            if (hashIndex < 0 || hashIndex > this.count)
                throw new ArgumentOutOfRangeException("hashIndex", string.Format("hashIndex must be between 0 and {0}", this.count)); 

            int start = hashIndex * HashCodeLength;
            for (int i = 0; i < HashCodeLength; i++)
                if (hash[i] != this.hashData[i + start])
                    return false;

            return true;
        }

        /// <summary>
        /// Returns the hash for a specific piece
        /// </summary>
        /// <param name="hashIndex">Piece/hash index to return</param>
        /// <returns>byte[] (length HashCodeLength) containing hashdata</returns>
        public byte[] ReadHash(int hashIndex)
        {
            if (hashIndex < 0 || hashIndex >= this.count)
                throw new ArgumentOutOfRangeException("hashIndex");

            // Read out our specified piece's hash data
            byte[] hash = new byte[HashCodeLength];
            Buffer.BlockCopy(this.hashData, hashIndex * HashCodeLength, hash, 0, HashCodeLength);

            return hash;
        }

        #endregion Methods
    }
}
