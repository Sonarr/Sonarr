using System;

namespace MonoTorrent.BEncoding
{
    /// <summary>
    /// Class representing a BEncoded number
    /// </summary>
    public class BEncodedNumber : BEncodedValue, IComparable<BEncodedNumber>
    {
        #region Member Variables
        /// <summary>
        /// The value of the BEncodedNumber
        /// </summary>
        public long Number
        {
            get { return number; }
            set { number = value; }
        }
        internal long number;
        #endregion


        #region Constructors
        public BEncodedNumber()
            : this(0)
        {
        }

        /// <summary>
        /// Create a new BEncoded number with the given value
        /// </summary>
        /// <param name="initialValue">The inital value of the BEncodedNumber</param>
        public BEncodedNumber(long value)
        {
            this.number = value;
        }

        public static implicit operator BEncodedNumber(long value)
        {
            return new BEncodedNumber(value);
        }
        #endregion


        #region Encode/Decode Methods

        /// <summary>
        /// Encodes this number to the supplied byte[] starting at the supplied offset
        /// </summary>
        /// <param name="buffer">The buffer to write the data to</param>
        /// <param name="offset">The offset to start writing the data at</param>
        /// <returns></returns>
        public override int Encode(byte[] buffer, int offset)
        {
            long number = this.number;

            int written = offset;
            buffer[written++] = (byte)'i';
            
            if (number < 0)
            {
                buffer[written++] = (byte)'-';
                number = -number;
            }
            // Reverse the number '12345' to get '54321'
            long reversed = 0;
            for (long i = number; i != 0; i /= 10)
                reversed = reversed * 10 + i % 10;

            // Write each digit of the reversed number to the array. We write '1'
            // first, then '2', etc
            for (long i = reversed; i != 0; i /= 10)
                buffer[written++] = (byte)(i % 10 + '0');

            if (number == 0)
                buffer[written++] = (byte)'0';

            // If the original number ends in one or more zeros, they are lost
            // when we reverse the number. We add them back in here.
            for (long i = number; i % 10 == 0 && number != 0; i /= 10)
                buffer[written++] = (byte)'0';

            buffer[written++] = (byte)'e';
            return written - offset;
        }


        /// <summary>
        /// Decodes a BEncoded number from the supplied RawReader
        /// </summary>
        /// <param name="reader">RawReader containing a BEncoded Number</param>
        internal override void DecodeInternal(RawReader reader)
        {
            int sign = 1;
            if (reader == null)
                throw new ArgumentNullException("reader");

            if (reader.ReadByte() != 'i')              // remove the leading 'i'
                throw new BEncodingException("Invalid data found. Aborting.");

            if (reader.PeekByte() == '-')
            {
                sign = -1;
                reader.ReadByte ();
            }

            int letter;
            while (((letter = reader.PeekByte()) != -1) && letter != 'e')
            {
                if(letter < '0' || letter > '9')
                    throw new BEncodingException("Invalid number found.");
                number = number * 10 + (letter - '0');
                reader.ReadByte ();
            }
            if (reader.ReadByte() != 'e')        //remove the trailing 'e'
                throw new BEncodingException("Invalid data found. Aborting.");

            number *= sign;
        }
        #endregion


        #region Helper Methods
        /// <summary>
        /// Returns the length of the encoded string in bytes
        /// </summary>
        /// <returns></returns>
        public override int LengthInBytes()
        {
            long number = this.number;
            int count = 2; // account for the 'i' and 'e'

            if (number == 0)
                return count + 1;

            if (number < 0)
            {
                number = -number;
                count++;
            }
            for (long i = number; i != 0; i /= 10)
                count++;

            return count;
        }


        public int CompareTo(object other)
        {
            if (other is BEncodedNumber || other is long || other is int)
                return CompareTo((BEncodedNumber)other);

            return -1;
        }

        public int CompareTo(BEncodedNumber other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            return this.number.CompareTo(other.number);
        }


        public int CompareTo(long other)
        {
            return this.number.CompareTo(other);
        }
        #endregion


        #region Overridden Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            BEncodedNumber obj2 = obj as BEncodedNumber;
            if (obj2 == null)
                return false;

            return (this.number == obj2.number);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.number.GetHashCode();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (this.number.ToString());
        }
        #endregion
    }
}
