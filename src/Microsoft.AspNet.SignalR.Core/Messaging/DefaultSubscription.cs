// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace Microsoft.AspNet.SignalR.Messaging
{
    internal class DefaultSubscription : Subscription
    {
        private List<Cursor> _cursors;
        private List<Topic> _cursorTopics;

        private readonly IStringMinifier _stringMinifier;

        public DefaultSubscription(string identity,
                                   IList<string> eventKeys,
                                   TopicLookup topics,
                                   string cursor,
                                   Func<MessageResult, object, Task<bool>> callback,
                                   int maxMessages,
                                   IStringMinifier stringMinifier,
                                   IPerformanceCounterManager counters,
                                   object state) :
            base(identity, eventKeys, callback, maxMessages, counters, state)
        {
            _stringMinifier = stringMinifier;

            if (String.IsNullOrEmpty(cursor))
            {
                _cursors = GetCursorsFromEventKeys(EventKeys, topics);
            }
            else
            {
                // Ensure delegate continues to use the C# Compiler static delegate caching optimization.
                _cursors = Cursor.GetCursors(cursor, (k, s) => UnminifyCursor(k, s), stringMinifier) ?? GetCursorsFromEventKeys(EventKeys, topics);
            }

            _cursorTopics = new List<Topic>();

            if (!String.IsNullOrEmpty(cursor))
            {
                // Update all of the cursors so we're within the range
                for (int i = _cursors.Count - 1; i >= 0; i--)
                {
                    Cursor c = _cursors[i];
                    Topic topic;
                    if (!EventKeys.Contains(c.Key))
                    {
                        _cursors.Remove(c);
                    }
                    else if (!topics.TryGetValue(_cursors[i].Key, out topic) || _cursors[i].Id > topic.Store.GetMessageCount())
                    {
                        UpdateCursor(c.Key, 0);
                    }
                }
            }

            // Add dummy entries so they can be filled in
            for (int i = 0; i < _cursors.Count; i++)
            {
                _cursorTopics.Add(null);
            }
        }

        private static string UnminifyCursor(string key, object state)
        {
            return ((IStringMinifier)state).Unminify(key);
        }

        public override bool AddEvent(string eventKey, Topic topic)
        {
            base.AddEvent(eventKey, topic);

            lock (_cursors)
            {
                // O(n), but small n and it's not common
                var index = _cursors.FindIndex(c => c.Key == eventKey);
                if (index == -1)
                {
                    _cursors.Add(new Cursor(eventKey, GetMessageId(topic), _stringMinifier.Minify(eventKey)));

                    _cursorTopics.Add(topic);

                    return true;
                }

                return false;
            }
        }

        public override void RemoveEvent(string eventKey)
        {
            base.RemoveEvent(eventKey);

            lock (_cursors)
            {
                var index = _cursors.FindIndex(c => c.Key == eventKey);
                if (index != -1)
                {
                    _cursors.RemoveAt(index);
                    _cursorTopics.RemoveAt(index);
                }
            }
        }

        public override void SetEventTopic(string eventKey, Topic topic)
        {
            base.SetEventTopic(eventKey, topic);

            lock (_cursors)
            {
                // O(n), but small n and it's not common
                var index = _cursors.FindIndex(c => c.Key == eventKey);
                if (index != -1)
                {
                    _cursorTopics[index] = topic;
                }
            }
        }

        public override void WriteCursor(TextWriter textWriter)
        {
            lock (_cursors)
            {
                Cursor.WriteCursors(textWriter, _cursors);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "It is called from the base class")]
        protected override void PerformWork(IList<ArraySegment<Message>> items, out int totalCount, out object state)
        {
            totalCount = 0;

            lock (_cursors)
            {
                var cursors = new ulong[_cursors.Count];
                for (int i = 0; i < _cursors.Count; i++)
                {
                    MessageStoreResult<Message> storeResult = _cursorTopics[i].Store.GetMessages(_cursors[i].Id, MaxMessages);
                    cursors[i] = storeResult.FirstMessageId + (ulong)storeResult.Messages.Count;

                    if (storeResult.Messages.Count > 0)
                    {
                        items.Add(storeResult.Messages);
                        totalCount += storeResult.Messages.Count;
                    }
                }

                // Return the state as a list of cursors
                state = cursors;
            }
        }

        protected override void BeforeInvoke(object state)
        {
            lock (_cursors)
            {
                // Update the list of cursors before invoking anything
                var nextCursors = (ulong[])state;
                for (int i = 0; i < _cursors.Count; i++)
                {
                    _cursors[i].Id = nextCursors[i];
                }
            }
        }

        private bool UpdateCursor(string key, ulong id)
        {
            lock (_cursors)
            {
                // O(n), but small n and it's not common
                var index = _cursors.FindIndex(c => c.Key == key);
                if (index != -1)
                {
                    _cursors[index].Id = id;
                    return true;
                }

                return false;
            }
        }

        private List<Cursor> GetCursorsFromEventKeys(IList<string> eventKeys, TopicLookup topics)
        {
            var list = new List<Cursor>(eventKeys.Count);
            foreach (var eventKey in eventKeys)
            {
                var cursor = new Cursor(eventKey, GetMessageId(topics, eventKey), _stringMinifier.Minify(eventKey));
                list.Add(cursor);
            }

            return list;
        }

        private static ulong GetMessageId(TopicLookup topics, string key)
        {
            Topic topic;
            if (topics.TryGetValue(key, out topic))
            {
                return GetMessageId(topic);
            }
            return 0;
        }

        private static ulong GetMessageId(Topic topic)
        {
            if (topic == null)
            {
                return 0;
            }

            return topic.Store.GetMessageCount();
        }
    }
}
