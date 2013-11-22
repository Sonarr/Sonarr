// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Microsoft.AspNet.SignalR.Hosting;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    /// <summary>
    /// TextWriter implementation over a write delegate optimized for writing in small chunks
    /// we don't need to write to a long lived buffer. This saves massive amounts of memory
    /// as the number of connections grows.
    /// </summary>
    internal unsafe class BufferTextWriter : TextWriter, IBinaryWriter
    {
        private readonly Encoding _encoding;

        private readonly Action<ArraySegment<byte>, object> _write;
        private readonly object _writeState;
        private readonly bool _reuseBuffers;

        private ChunkedWriter _writer;
        private int _bufferSize;

        public BufferTextWriter(IResponse response) :
            this((data, state) => ((IResponse)state).Write(data), response, reuseBuffers: true, bufferSize: 128)
        {

        }

        public BufferTextWriter(IWebSocket socket) :
            this((data, state) => ((IWebSocket)state).SendChunk(data), socket, reuseBuffers: false, bufferSize: 128)
        {

        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.IO.TextWriter.#ctor", Justification = "It won't be used")]
        public BufferTextWriter(Action<ArraySegment<byte>, object> write, object state, bool reuseBuffers, int bufferSize)
        {
            _write = write;
            _writeState = state;
            _encoding = new UTF8Encoding();
            _reuseBuffers = reuseBuffers;
            _bufferSize = bufferSize;
        }

        private ChunkedWriter Writer
        {
            get
            {
                if (_writer == null)
                {
                    _writer = new ChunkedWriter(_write, _writeState, _bufferSize, _encoding, _reuseBuffers);
                }

                return _writer;
            }
        }

        public override Encoding Encoding
        {
            get { return _encoding; }
        }

        public override void Write(string value)
        {
            Writer.Write(value);
        }

        public override void WriteLine(string value)
        {
            Writer.Write(value);
        }

        public override void Write(char value)
        {
            Writer.Write(value);
        }

        public void Write(ArraySegment<byte> data)
        {
            Writer.Write(data);
        }

        public override void Flush()
        {
            Writer.Flush();
        }

        private class ChunkedWriter
        {
            private int _charPos;
            private int _charLen;

            private readonly Encoder _encoder;
            private readonly char[] _charBuffer;
            private readonly byte[] _byteBuffer;
            private readonly Action<ArraySegment<byte>, object> _write;
            private readonly object _writeState;

            public ChunkedWriter(Action<ArraySegment<byte>, object> write, object state, int chunkSize, Encoding encoding, bool reuseBuffers)
            {
                _charLen = chunkSize;
                _charBuffer = new char[chunkSize];
                _write = write;
                _writeState = state;
                _encoder = encoding.GetEncoder();

                if (reuseBuffers)
                {
                    _byteBuffer = new byte[encoding.GetMaxByteCount(chunkSize)];
                }
            }

            public void Write(char value)
            {
                if (_charPos == _charLen)
                {
                    Flush(flushEncoder: false);
                }

                _charBuffer[_charPos++] = value;
            }

            public void Write(string value)
            {
                int length = value.Length;
                int sourceIndex = 0;

                while (length > 0)
                {
                    if (_charPos == _charLen)
                    {
                        Flush(flushEncoder: false);
                    }

                    int count = _charLen - _charPos;
                    if (count > length)
                    {
                        count = length;
                    }

                    value.CopyTo(sourceIndex, _charBuffer, _charPos, count);
                    _charPos += count;
                    sourceIndex += count;
                    length -= count;
                }
            }

            public void Write(ArraySegment<byte> data)
            {
                Flush();
                _write(data, _writeState);
            }

            public void Flush()
            {
                Flush(flushEncoder: true);
            }

            private void Flush(bool flushEncoder)
            {
                // If it's safe to reuse the buffer then do so
                if (_byteBuffer != null)
                {
                    Flush(_byteBuffer, flushEncoder);
                }
                else
                {
                    // Allocate a byte array of the right size for this char buffer
                    int byteCount = _encoder.GetByteCount(_charBuffer, 0, _charPos, flush: false);
                    var byteBuffer = new byte[byteCount];
                    Flush(byteBuffer, flushEncoder);
                }
            }

            private void Flush(byte[] byteBuffer, bool flushEncoder)
            {
                int count = _encoder.GetBytes(_charBuffer, 0, _charPos, byteBuffer, 0, flush: flushEncoder);

                _charPos = 0;

                if (count > 0)
                {
                    _write(new ArraySegment<byte>(byteBuffer, 0, count), _writeState);
                }
            }
        }
    }
}
