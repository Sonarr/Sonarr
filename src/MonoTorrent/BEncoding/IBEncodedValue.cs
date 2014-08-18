using System;
using System.IO;
using System.Text;

namespace MonoTorrent.BEncoding
{
    /// <summary>
    /// Base interface for all BEncoded values.
    /// </summary>
    public abstract class BEncodedValue
    {
        internal abstract void DecodeInternal(RawReader reader);

        /// <summary>
        /// Encodes the BEncodedValue into a byte array
        /// </summary>
        /// <returns>Byte array containing the BEncoded Data</returns>
        public byte[] Encode()
        {
            byte[] buffer = new byte[LengthInBytes()];
            if (Encode(buffer, 0) != buffer.Length)
                throw new BEncodingException("Error encoding the data");

            return buffer;
        }


        /// <summary>
        /// Encodes the BEncodedValue into the supplied buffer
        /// </summary>
        /// <param name="buffer">The buffer to encode the information to</param>
        /// <param name="offset">The offset in the buffer to start writing the data</param>
        /// <returns></returns>
        public abstract int Encode(byte[] buffer, int offset);

        public static T Clone <T> (T value)
            where T : BEncodedValue
        {
            Check.Value (value);
            return (T) BEncodedValue.Decode (value.Encode ());
        }

        /// <summary>
        /// Interface for all BEncoded values
        /// </summary>
        /// <param name="data">The byte array containing the BEncoded data</param>
        /// <returns></returns>
        public static BEncodedValue Decode(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            using (RawReader stream = new RawReader(new MemoryStream(data)))
                return (Decode(stream));
        }

        internal static BEncodedValue Decode(byte[] buffer, bool strictDecoding)
        {
            return Decode(buffer, 0, buffer.Length, strictDecoding);
        }

        /// <summary>
        /// Decode BEncoded data in the given byte array
        /// </summary>
        /// <param name="buffer">The byte array containing the BEncoded data</param>
        /// <param name="offset">The offset at which the data starts at</param>
        /// <param name="length">The number of bytes to be decoded</param>
        /// <returns>BEncodedValue containing the data that was in the byte[]</returns>
        public static BEncodedValue Decode(byte[] buffer, int offset, int length)
        {
            return Decode(buffer, offset, length, true);
        }

        public static BEncodedValue Decode(byte[] buffer, int offset, int length, bool strictDecoding)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (offset < 0 || length < 0)
                throw new IndexOutOfRangeException("Neither offset or length can be less than zero");

            if (offset > buffer.Length - length)
                throw new ArgumentOutOfRangeException("length");

            using (RawReader reader = new RawReader(new MemoryStream(buffer, offset, length), strictDecoding))
                return (BEncodedValue.Decode(reader));
        }


        /// <summary>
        /// Decode BEncoded data in the given stream 
        /// </summary>
        /// <param name="stream">The stream containing the BEncoded data</param>
        /// <returns>BEncodedValue containing the data that was in the stream</returns>
        public static BEncodedValue Decode(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            return Decode(new RawReader(stream));
        }


        /// <summary>
        /// Decode BEncoded data in the given RawReader
        /// </summary>
        /// <param name="reader">The RawReader containing the BEncoded data</param>
        /// <returns>BEncodedValue containing the data that was in the stream</returns>
        public static BEncodedValue Decode(RawReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            BEncodedValue data;
            switch (reader.PeekByte())
            {
                case ('i'):                         // Integer
                    data = new BEncodedNumber();
                    break;

                case ('d'):                         // Dictionary
                    data = new BEncodedDictionary();
                    break;

                case ('l'):                         // List
                    data = new BEncodedList();
                    break;

                case ('1'):                         // String
                case ('2'):
                case ('3'):
                case ('4'):
                case ('5'):
                case ('6'):
                case ('7'):
                case ('8'):
                case ('9'):
                case ('0'):
                    data = new BEncodedString();
                    break;

                default:
                    throw new BEncodingException("Could not find what value to decode");
            }

            data.DecodeInternal(reader);
            return data;
        }


        /// <summary>
        /// Interface for all BEncoded values
        /// </summary>
        /// <param name="data">The byte array containing the BEncoded data</param>
        /// <returns></returns>
        public static T Decode<T>(byte[] data) where T : BEncodedValue
        {
            return (T)BEncodedValue.Decode(data);
        }


        /// <summary>
        /// Decode BEncoded data in the given byte array
        /// </summary>
        /// <param name="buffer">The byte array containing the BEncoded data</param>
        /// <param name="offset">The offset at which the data starts at</param>
        /// <param name="length">The number of bytes to be decoded</param>
        /// <returns>BEncodedValue containing the data that was in the byte[]</returns>
        public static T Decode<T>(byte[] buffer, int offset, int length) where T : BEncodedValue
        {
            return BEncodedValue.Decode<T>(buffer, offset, length, true);
        }

        public static T Decode<T>(byte[] buffer, int offset, int length, bool strictDecoding) where T : BEncodedValue
        {
            return (T)BEncodedValue.Decode(buffer, offset, length, strictDecoding);
        }


        /// <summary>
        /// Decode BEncoded data in the given stream 
        /// </summary>
        /// <param name="stream">The stream containing the BEncoded data</param>
        /// <returns>BEncodedValue containing the data that was in the stream</returns>
        public static T Decode<T>(Stream stream) where T : BEncodedValue
        {
            return (T)BEncodedValue.Decode(stream);
        }


        public static T Decode<T>(RawReader reader) where T : BEncodedValue
        {
            return (T)BEncodedValue.Decode(reader);
        }


        /// <summary>
        /// Returns the size of the byte[] needed to encode this BEncodedValue
        /// </summary>
        /// <returns></returns>
        public abstract int LengthInBytes();
    }
}
