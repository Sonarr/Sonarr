using System;
using Microsoft.AspNet.SignalR.Hosting;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    /// <summary>
    /// A buffering text writer that supports writing binary directly as well
    /// </summary>
    internal unsafe class BinaryTextWriter : BufferTextWriter, IBinaryWriter
    {
        public BinaryTextWriter(IResponse response) :
            base((data, state) => ((IResponse)state).Write(data), response, reuseBuffers: true, bufferSize: 128)
        {

        }

        public BinaryTextWriter(IWebSocket socket) :
            base((data, state) => ((IWebSocket)state).SendChunk(data), socket, reuseBuffers: false, bufferSize: 1024)
        {

        }


        public BinaryTextWriter(Action<ArraySegment<byte>, object> write, object state, bool reuseBuffers, int bufferSize) :
            base(write, state, reuseBuffers, bufferSize)
        {
        }

        public void Write(ArraySegment<byte> data)
        {
            Writer.Write(data);
        }
    }
}
