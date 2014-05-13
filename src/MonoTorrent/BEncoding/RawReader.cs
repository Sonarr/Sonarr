using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MonoTorrent.BEncoding
{
    public class RawReader : Stream
    {
        bool hasPeek;
        Stream input;
        byte[] peeked;
        bool strictDecoding;

        public bool StrictDecoding
        {
            get { return strictDecoding; }
        }

        public RawReader(Stream input)
            : this(input, true)
        {

        }

        public RawReader(Stream input, bool strictDecoding)
        {
            this.input = input;
            this.peeked = new byte[1];
            this.strictDecoding = strictDecoding;
        }

        public override bool CanRead
        {
            get { return input.CanRead; }
        }

        public override bool CanSeek
        {
            get { return input.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Length
        {
            get { return input.Length; }
        }

        public int PeekByte()
        {
            if (!hasPeek)
                hasPeek = Read(peeked, 0, 1) == 1;
            return hasPeek ? peeked[0] : -1;
        }

        public override int ReadByte()
        {
            if (hasPeek)
            {
                hasPeek = false;
                return peeked[0];
            }
            return base.ReadByte();
        }

        public override long Position
        {
            get
            {
                if (hasPeek)
                    return input.Position - 1;
                return input.Position;
            }
            set
            {
                if (value != Position)
                {
                    hasPeek = false;
                    input.Position = value;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = 0;
            if (hasPeek && count > 0)
            {
                hasPeek = false;
                buffer[offset] = peeked[0];
                offset++;
                count--;
                read++;
            }
            read += input.Read(buffer, offset, count);
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long val;
            if (hasPeek && origin == SeekOrigin.Current)
                val = input.Seek(offset - 1, origin);
            else
                val = input.Seek(offset, origin);
            hasPeek = false;
            return val;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
