using System;
using System.Collections.Generic;
using System.Text;
using MonoTorrent.Common;
using System.Web;

namespace MonoTorrent
{
    public class InfoHash : IEquatable <InfoHash>
    {
        static Dictionary<char, byte> base32DecodeTable;

        static InfoHash()
        {
            base32DecodeTable = new Dictionary<char, byte>();
            string table = "abcdefghijklmnopqrstuvwxyz234567";
            for (int i = 0; i < table.Length; i++)
                base32DecodeTable[table[i]] = (byte)i;
        }

        byte[] hash;

        internal byte[] Hash
        {
            get { return hash; }
        }

        public InfoHash(byte[] infoHash)
        {
            Check.InfoHash(infoHash);
            if (infoHash.Length != 20)
                throw new ArgumentException("Infohash must be exactly 20 bytes long");
            hash = (byte[])infoHash.Clone();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as InfoHash);
        }

        public bool Equals(byte[] other)
        {
            return other == null || other.Length != 20 ? false : Toolbox.ByteMatch(Hash, other);
        }

        public bool Equals(InfoHash other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            // Equality is based generally on checking 20 positions, checking 4 should be enough
            // for the hashcode as infohashes are randomly distributed.
            return Hash[0] | (Hash[1] << 8) | (Hash[2] << 16) | (Hash[3] << 24);
        }

        public byte[] ToArray()
        {
            return (byte[])hash.Clone();
        }

        public string ToHex()
        {
            StringBuilder sb = new StringBuilder(40);
            for (int i = 0; i < hash.Length; i++)
            {
                string hex = hash[i].ToString("X");
                if (hex.Length != 2)
                    sb.Append("0");
                sb.Append(hex);
            }
            return sb.ToString();
        }

        public override string ToString()
        {
            return BitConverter.ToString(hash);
        }

        public string UrlEncode()
        {
            return UriHelper.UrlEncode(Hash);
        }

        public static bool operator ==(InfoHash left, InfoHash right)
        {
            if ((object)left == null)
                return (object)right == null;
            if ((object)right == null)
                return false;
            return Toolbox.ByteMatch(left.Hash, right.Hash);
        }

        public static bool operator !=(InfoHash left, InfoHash right)
        {
            return !(left == right);
        }

        public static InfoHash FromBase32(string infoHash)
        {
            Check.InfoHash (infoHash);
            if (infoHash.Length != 32)
                throw new ArgumentException("Infohash must be a base32 encoded 32 character string");
            
            infoHash = infoHash.ToLower();
            int infohashOffset =0 ;
            byte[] hash = new byte[20];
            var temp = new byte[8];
            for (int i = 0; i < hash.Length; ) {
                for (int j=0; j < 8; j++)
                    if (!base32DecodeTable.TryGetValue(infoHash[infohashOffset++], out temp[j]))
                        throw new ArgumentException ("infoHash", "Value is not a valid base32 encoded string");

                //8 * 5bits = 40 bits = 5 bytes
                hash[i++] = (byte)((temp[0] << 3) | (temp [1]>> 2));
                hash[i++] = (byte)((temp[1] << 6) | (temp[2] << 1) | (temp[3] >> 4));
                hash[i++] = (byte)((temp[3] << 4) | (temp [4]>> 1));
                hash[i++] = (byte)((temp[4] << 7) | (temp[5] << 2) | (temp [6]>> 3));
                hash[i++] = (byte)((temp[6] << 5) | temp[7]);
            }

            return new InfoHash(hash);
        }

        public static InfoHash FromHex(string infoHash)
        {
            Check.InfoHash (infoHash);
            if (infoHash.Length != 40)
                throw new ArgumentException("Infohash must be 40 characters long");
            
            byte[] hash = new byte[20];
            for (int i = 0; i < hash.Length; i++)
                hash[i] = byte.Parse(infoHash.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);

            return new InfoHash(hash);
        }

        public static InfoHash FromMagnetLink(string magnetLink)
        {
            Check.MagnetLink(magnetLink);
            if (!magnetLink.StartsWith("magnet:?"))
                throw new ArgumentException("Invalid magnet link format");
            magnetLink = magnetLink.Substring("magnet:?".Length);
            int hashStart = magnetLink.IndexOf("xt=urn:btih:");
            if (hashStart == -1)
                throw new ArgumentException("Magnet link does not contain an infohash");
            hashStart += "xt=urn:btih:".Length;

            int hashEnd = magnetLink.IndexOf('&', hashStart);
            if (hashEnd == -1)
                hashEnd = magnetLink.Length;

            switch (hashEnd - hashStart)
            {
                case 32:
                    return FromBase32(magnetLink.Substring(hashStart, 32));
                case 40:
                    return FromHex(magnetLink.Substring(hashStart, 40));
                default:
                    throw new ArgumentException("Infohash must be base32 or hex encoded.");
            }
        }

        public static InfoHash UrlDecode(string infoHash)
        {
            Check.InfoHash(infoHash);
            return new InfoHash(UriHelper.UrlDecode(infoHash));
        }
    }
}
