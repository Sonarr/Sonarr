// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.IO;
using System.Text;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    public class ArraySegmentTextReader : TextReader
    {
        private readonly ArraySegment<byte> _buffer;
        private readonly Encoding _encoding;
        private int _offset;

        public ArraySegmentTextReader(ArraySegment<byte> buffer, Encoding encoding)
        {
            _buffer = buffer;
            _encoding = encoding;
            _offset = _buffer.Offset;
        }

        public override int Read(char[] buffer, int index, int count)
        {
            int bytesCount = _encoding.GetByteCount(buffer, index, count);
            int bytesToRead = Math.Min(_buffer.Count - _offset, bytesCount);

            int read = _encoding.GetChars(_buffer.Array, _offset, bytesToRead, buffer, index);
            _offset += bytesToRead;

            return read;
        }
    }
}
