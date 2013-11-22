using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;

namespace Microsoft.AspNet.SignalR.Messaging
{
    /// <summary>
    /// Represents a message to the scaleout backplane
    /// </summary>
    public class ScaleoutMessage
    {
        public ScaleoutMessage(IList<Message> messages)
        {
            Messages = messages;
            ServerCreationTime = DateTime.UtcNow;
        }

        public ScaleoutMessage()
        {
        }

        /// <summary>
        /// The messages from SignalR
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This type is used for serialization")]
        public IList<Message> Messages { get; set; }

        /// <summary>
        /// The time the message was created on the origin server
        /// </summary>
        public DateTime ServerCreationTime { get; set; }

        public byte[] ToBytes()
        {
            using (var ms = new MemoryStream())
            {
                var binaryWriter = new BinaryWriter(ms);

                binaryWriter.Write(Messages.Count);
                for (int i = 0; i < Messages.Count; i++)
                {
                    Messages[i].WriteTo(ms);
                }
                binaryWriter.Write(ServerCreationTime.Ticks);

                return ms.ToArray();
            }
        }

        public static ScaleoutMessage FromBytes(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            using (var stream = new MemoryStream(data))
            {
                var binaryReader = new BinaryReader(stream);
                var message = new ScaleoutMessage();
                message.Messages = new List<Message>();
                int count = binaryReader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    message.Messages.Add(Message.ReadFrom(stream));
                }
                message.ServerCreationTime = new DateTime(binaryReader.ReadInt64());

                return message;
            }
        }
    }
}
