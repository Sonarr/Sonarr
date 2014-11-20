using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace MonoTorrent.BEncoding
{
    /// <summary>
    /// Class representing a BEncoded Dictionary
    /// </summary>
    public class BEncodedDictionary : BEncodedValue, IDictionary<BEncodedString, BEncodedValue>
    {
        #region Member Variables

        private SortedDictionary<BEncodedString, BEncodedValue> dictionary;

        #endregion


        #region Constructors

        /// <summary>
        /// Create a new BEncodedDictionary
        /// </summary>
        public BEncodedDictionary()
        {
            this.dictionary = new SortedDictionary<BEncodedString, BEncodedValue>();
        }

        #endregion


        #region Encode/Decode Methods

        /// <summary>
        /// Encodes the dictionary to a byte[]
        /// </summary>
        /// <param name="buffer">The buffer to encode the data to</param>
        /// <param name="offset">The offset to start writing the data to</param>
        /// <returns></returns>
        public override int Encode(byte[] buffer, int offset)
        {
            int written = 0;

            //Dictionaries start with 'd'
            buffer[offset] = (byte)'d';
            written++;

            foreach (KeyValuePair<BEncodedString, BEncodedValue> keypair in this)
            {
                written += keypair.Key.Encode(buffer, offset + written);
                written += keypair.Value.Encode(buffer, offset + written);
            }

            // Dictionaries end with 'e'
            buffer[offset + written] = (byte)'e';
            written++;
            return written;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        internal override void DecodeInternal(RawReader reader)
        {
            DecodeInternal(reader, reader.StrictDecoding);
        }

        private void DecodeInternal(RawReader reader, bool strictDecoding)
        {
            BEncodedString key = null;
            BEncodedValue value = null;
            BEncodedString oldkey = null;

            if (reader.ReadByte() != 'd')
                throw new BEncodingException("Invalid data found. Aborting"); // Remove the leading 'd'

            while ((reader.PeekByte() != -1) && (reader.PeekByte() != 'e'))
            {
                key = (BEncodedString)BEncodedValue.Decode(reader);         // keys have to be BEncoded strings

                if (oldkey != null && oldkey.CompareTo(key) > 0)
                    if (strictDecoding)
                        throw new BEncodingException(String.Format(
                            "Illegal BEncodedDictionary. The attributes are not ordered correctly. Old key: {0}, New key: {1}",
                            oldkey, key));

                oldkey = key;
                value = BEncodedValue.Decode(reader);                     // the value is a BEncoded value
                dictionary.Add(key, value);
            }

            if (reader.ReadByte() != 'e')                                    // remove the trailing 'e'
                throw new BEncodingException("Invalid data found. Aborting");
        }

        public static BEncodedDictionary DecodeTorrent(byte[] bytes)
        {
            return DecodeTorrent(new MemoryStream(bytes));
        }

        public static BEncodedDictionary DecodeTorrent(Stream s)
        {
            return DecodeTorrent(new RawReader(s));
        }


        /// <summary>
        /// Special decoding method for torrent files - allows dictionary attributes to be out of order for the
        /// overall torrent file, but imposes strict rules on the info dictionary.
        /// </summary>
        /// <returns></returns>
        public static BEncodedDictionary DecodeTorrent(RawReader reader)
        {
            BEncodedString key = null;
            BEncodedValue value = null;
            BEncodedDictionary torrent = new BEncodedDictionary();
            if (reader.ReadByte() != 'd')
                throw new BEncodingException("Invalid data found. Aborting"); // Remove the leading 'd'

            while ((reader.PeekByte() != -1) && (reader.PeekByte() != 'e'))
            {
                key = (BEncodedString)BEncodedValue.Decode(reader);         // keys have to be BEncoded strings

                if (reader.PeekByte() == 'd')
                {
                    value = new BEncodedDictionary();
                    if (key.Text.ToLower().Equals("info"))
                        ((BEncodedDictionary)value).DecodeInternal(reader, true);
                    else
                        ((BEncodedDictionary)value).DecodeInternal(reader, false);
                }
                else
                    value = BEncodedValue.Decode(reader);                     // the value is a BEncoded value
                    
                torrent.dictionary.Add(key, value);
            }

            if (reader.ReadByte() != 'e')                                    // remove the trailing 'e'
                throw new BEncodingException("Invalid data found. Aborting");

            return torrent;
        }

        #endregion


        #region Helper Methods

        /// <summary>
        /// Returns the size of the dictionary in bytes using UTF8 encoding
        /// </summary>
        /// <returns></returns>
        public override int LengthInBytes()
        {
            int length = 0;
            length += 1;   // Dictionaries start with 'd'

            foreach (KeyValuePair<BEncodedString, BEncodedValue> keypair in this.dictionary)
            {
                length += keypair.Key.LengthInBytes();
                length += keypair.Value.LengthInBytes();
            }
            length += 1;   // Dictionaries end with 'e'
            return length;
        }

        #endregion


        #region Overridden Methods
        public override bool Equals(object obj)
        {
            BEncodedValue val;
            BEncodedDictionary other = obj as BEncodedDictionary;
            if (other == null)
                return false;

            if (this.dictionary.Count != other.dictionary.Count)
                return false;

            foreach (KeyValuePair<BEncodedString, BEncodedValue> keypair in this.dictionary)
            {
                if (!other.TryGetValue(keypair.Key, out val))
                    return false;

                if (!keypair.Value.Equals(val))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int result = 0;
            foreach (KeyValuePair<BEncodedString, BEncodedValue> keypair in dictionary)
            {
                result ^= keypair.Key.GetHashCode();
                result ^= keypair.Value.GetHashCode();
            }

            return result;
        }

        public override string ToString()
        {
            return System.Text.Encoding.UTF8.GetString(Encode());
        }
        #endregion


        #region IDictionary and IList methods
        public void Add(BEncodedString key, BEncodedValue value)
        {
            this.dictionary.Add(key, value);
        }

        public void Add(KeyValuePair<BEncodedString, BEncodedValue> item)
        {
            this.dictionary.Add(item.Key, item.Value);
        }
        public void Clear()
        {
            this.dictionary.Clear();
        }

        public bool Contains(KeyValuePair<BEncodedString, BEncodedValue> item)
        {
            if (!this.dictionary.ContainsKey(item.Key))
                return false;

            return this.dictionary[item.Key].Equals(item.Value);
        }

        public bool ContainsKey(BEncodedString key)
        {
            return this.dictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<BEncodedString, BEncodedValue>[] array, int arrayIndex)
        {
            this.dictionary.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this.dictionary.Count; }
        }

        //public int IndexOf(KeyValuePair<BEncodedString, IBEncodedValue> item)
        //{
        //    return this.dictionary.IndexOf(item);
        //}

        //public void Insert(int index, KeyValuePair<BEncodedString, IBEncodedValue> item)
        //{
        //    this.dictionary.Insert(index, item);
        //}

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(BEncodedString key)
        {
            return this.dictionary.Remove(key);
        }

        public bool Remove(KeyValuePair<BEncodedString, BEncodedValue> item)
        {
            return this.dictionary.Remove(item.Key);
        }

        //public void RemoveAt(int index)
        //{
        //    this.dictionary.RemoveAt(index);
        //}

        public bool TryGetValue(BEncodedString key, out BEncodedValue value)
        {
            return this.dictionary.TryGetValue(key, out value);
        }

        public BEncodedValue this[BEncodedString key]
        {
            get { return this.dictionary[key]; }
            set { this.dictionary[key] = value; }
        }

        //public KeyValuePair<BEncodedString, IBEncodedValue> this[int index]
        //{
        //    get { return this.dictionary[index]; }
        //    set { this.dictionary[index] = value; }
        //}

        public ICollection<BEncodedString> Keys
        {
            get { return this.dictionary.Keys; }
        }

        public ICollection<BEncodedValue> Values
        {
            get { return this.dictionary.Values; }
        }

        public IEnumerator<KeyValuePair<BEncodedString, BEncodedValue>> GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }
        #endregion
    }
}