// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.AspNet.SignalR.Json;
using Microsoft.AspNet.SignalR.Messaging;
using Newtonsoft.Json;

namespace Microsoft.AspNet.SignalR.Transports
{
    /// <summary>
    /// Represents a response to a connection.
    /// </summary>
    public sealed class PersistentResponse : IJsonWritable
    {
        private readonly Func<Message, bool> _exclude;
        private readonly Action<TextWriter> _writeCursor;

        public PersistentResponse()
            : this(message => false, writer => { })
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="PersistentResponse"/>.
        /// </summary>
        /// <param name="exclude">A filter that determines whether messages should be written to the client.</param>
        /// <param name="writeCursor">The cursor writer.</param>
        public PersistentResponse(Func<Message, bool> exclude, Action<TextWriter> writeCursor)
        {
            _exclude = exclude;
            _writeCursor = writeCursor;
        }

        /// <summary>
        /// The list of messages to be sent to the receiving connection.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an optimization and this type is only used for serialization.")]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This type is only used for serialization")]
        public IList<ArraySegment<Message>> Messages { get; set; }

        public bool Terminal { get; set; }

        /// <summary>
        /// The total count of the messages sent the receiving connection.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// True if the connection receives a disconnect command.
        /// </summary>
        public bool Disconnect { get; set; }

        /// <summary>
        /// True if the connection was forcibly closed. 
        /// </summary>
        public bool Aborted { get; set; }

        /// <summary>
        /// True if the client should try reconnecting.
        /// </summary>
        // This is set when the host is shutting down.
        public bool Reconnect { get; set; }

        /// <summary>
        /// Signed token representing the list of groups. Updates on change.
        /// </summary>
        public string GroupsToken { get; set; }

        /// <summary>
        /// The time the long polling client should wait before reestablishing a connection if no data is received.
        /// </summary>
        public long? LongPollDelay { get; set; }

        /// <summary>
        /// Serializes only the necessary components of the <see cref="PersistentResponse"/> to JSON
        /// using Json.NET's JsonTextWriter to improve performance.
        /// </summary>
        /// <param name="writer">The <see cref="System.IO.TextWriter"/> that receives the JSON serialization.</param>
        void IJsonWritable.WriteJson(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            var jsonWriter = new JsonTextWriter(writer);
            jsonWriter.WriteStartObject();

            // REVIEW: Is this 100% correct?
            writer.Write('"');
            writer.Write("C");
            writer.Write('"');
            writer.Write(':');
            writer.Write('"');
            _writeCursor(writer);
            writer.Write('"');
            writer.Write(',');

            if (Disconnect)
            {
                jsonWriter.WritePropertyName("D");
                jsonWriter.WriteValue(1);
            }

            if (Reconnect)
            {
                jsonWriter.WritePropertyName("T");
                jsonWriter.WriteValue(1);
            }

            if (GroupsToken != null)
            {
                jsonWriter.WritePropertyName("G");
                jsonWriter.WriteValue(GroupsToken);
            }

            if (LongPollDelay.HasValue)
            {
                jsonWriter.WritePropertyName("L");
                jsonWriter.WriteValue(LongPollDelay.Value);
            }

            jsonWriter.WritePropertyName("M");
            jsonWriter.WriteStartArray();

            WriteMessages(writer, jsonWriter);

            jsonWriter.WriteEndArray();
            jsonWriter.WriteEndObject();
        }

        private void WriteMessages(TextWriter writer, JsonTextWriter jsonWriter)
        {
            if (Messages == null)
            {
                return;
            }

            // If the writer is a binary writer then write to the underlying writer directly
            var binaryWriter = writer as IBinaryWriter;

            bool first = true;

            for (int i = 0; i < Messages.Count; i++)
            {
                ArraySegment<Message> segment = Messages[i];
                for (int j = segment.Offset; j < segment.Offset + segment.Count; j++)
                {
                    Message message = segment.Array[j];

                    if (!message.IsCommand && !_exclude(message))
                    {
                        if (binaryWriter != null)
                        {
                            if (!first)
                            {
                                // We need to write the array separator manually
                                writer.Write(',');
                            }

                            // If we can write binary then just write it
                            binaryWriter.Write(message.Value);

                            first = false;
                        }
                        else
                        {
                            // Write the raw JSON value
                            jsonWriter.WriteRawValue(message.GetString());
                        }
                    }
                }
            }
        }
    }
}
