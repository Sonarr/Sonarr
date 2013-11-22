// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace Microsoft.AspNet.SignalR.Messaging
{
    public sealed class TopicLookup : IEnumerable<KeyValuePair<string, Topic>>
    {
        // General topics
        private readonly ConcurrentDictionary<string, Topic> _topics = new ConcurrentDictionary<string, Topic>();

        // All group topics
        private readonly ConcurrentDictionary<string, Topic> _groupTopics = new ConcurrentDictionary<string, Topic>(new SipHashBasedStringEqualityComparer());

        public int Count
        {
            get
            {
                return _topics.Count + _groupTopics.Count;
            }
        }

        public Topic this[string key]
        {
            get
            {
                Topic topic;
                if (TryGetValue(key, out topic))
                {
                    return topic;
                }
                return null;
            }
        }

        public bool ContainsKey(string key)
        {
            if (PrefixHelper.HasGroupPrefix(key))
            {
                return _groupTopics.ContainsKey(key);
            }

            return _topics.ContainsKey(key);
        }

        public bool TryGetValue(string key, out Topic topic)
        {
            if (PrefixHelper.HasGroupPrefix(key))
            {
                return _groupTopics.TryGetValue(key, out topic);
            }

            return _topics.TryGetValue(key, out topic);
        }

        public IEnumerator<KeyValuePair<string, Topic>> GetEnumerator()
        {
            return _topics.Concat(_groupTopics).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool TryRemove(string key)
        {
            Topic topic;
            if (PrefixHelper.HasGroupPrefix(key))
            {
                return _groupTopics.TryRemove(key, out topic);
            }

            return _topics.TryRemove(key, out topic);
        }

        public Topic GetOrAdd(string key, Func<string, Topic> factory)
        {
            if (PrefixHelper.HasGroupPrefix(key))
            {
                return _groupTopics.GetOrAdd(key, factory);
            }

            return _topics.GetOrAdd(key, factory);
        }

        public void Clear()
        {
            _topics.Clear();
            _groupTopics.Clear();
        }
    }
}
