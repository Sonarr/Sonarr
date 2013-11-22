// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Threading;

namespace Microsoft.AspNet.SignalR.Messaging
{
    // Represents a message store that is backed by a ring buffer.
    public sealed class MessageStore<T> where T : class
    {
        private static readonly uint _minFragmentCount = 4;
        private static readonly uint _maxFragmentSize = (IntPtr.Size == 4) ? (uint)16384 : (uint)8192; // guarantees that fragments never end up in the LOH
        private static readonly ArraySegment<T> _emptyArraySegment = new ArraySegment<T>(new T[0]);
        private readonly uint _offset;

        private Fragment[] _fragments;
        private readonly uint _fragmentSize;

        private long _nextFreeMessageId;

        // Creates a message store with the specified capacity. The actual capacity will be *at least* the
        // specified value. That is, GetMessages may return more data than 'capacity'.
        public MessageStore(uint capacity, uint offset)
        {
            // set a minimum capacity
            if (capacity < 32)
            {
                capacity = 32;
            }

            _offset = offset;

            // Dynamically choose an appropriate number of fragments and the size of each fragment.
            // This is chosen to avoid allocations on the large object heap and to minimize contention
            // in the store. We allocate a small amount of additional space to act as an overflow
            // buffer; this increases throughput of the data structure.
            checked
            {
                uint fragmentCount = Math.Max(_minFragmentCount, capacity / _maxFragmentSize);
                _fragmentSize = Math.Min((capacity + fragmentCount - 1) / fragmentCount, _maxFragmentSize);
                _fragments = new Fragment[fragmentCount + 1]; // +1 for the overflow buffer
            }
        }

        public MessageStore(uint capacity)
            : this(capacity, offset: 0)
        {
        }

        // only for testing purposes
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Only for testing")]
        public ulong GetMessageCount()
        {
            return (ulong)Volatile.Read(ref _nextFreeMessageId);
        }

        // Adds a message to the store. Returns the ID of the newly added message.
        public ulong Add(T message)
        {
            // keep looping in TryAddImpl until it succeeds
            ulong newMessageId;
            while (!TryAddImpl(message, out newMessageId)) ;

            // When TryAddImpl succeeds, record the fact that a message was just added to the
            // store. We increment the next free id rather than set it explicitly since
            // multiple threads might be trying to write simultaneously. There is a nifty
            // side effect to this: _nextFreeMessageId will *always* return the total number
            // of messages that *all* threads agree have ever been added to the store. (The
            // actual number may be higher, but this field will eventually catch up as threads
            // flush data.)
            Interlocked.Increment(ref _nextFreeMessageId);
            return newMessageId;
        }

        private void GetFragmentOffsets(ulong messageId, out ulong fragmentNum, out int idxIntoFragmentsArray, out int idxIntoFragment)
        {
            fragmentNum = messageId / _fragmentSize;

            // from the bucket number, we can figure out where in _fragments this data sits
            idxIntoFragmentsArray = (int)(fragmentNum % (uint)_fragments.Length);
            idxIntoFragment = (int)(messageId % _fragmentSize);
        }

        private ulong GetMessageId(ulong fragmentNum, uint offset)
        {
            return fragmentNum * _fragmentSize + offset;
        }

        // Gets the next batch of messages, beginning with the specified ID.
        // This function may return an empty array or an array of length greater than the capacity
        // specified in the ctor. The client may also miss messages. See MessageStoreResult.
        public MessageStoreResult<T> GetMessages(ulong firstMessageId, int maxMessages)
        {
            return GetMessagesImpl(firstMessageId, maxMessages);
        }

        private MessageStoreResult<T> GetMessagesImpl(ulong firstMessageIdRequestedByClient, int maxMessages)
        {
            ulong nextFreeMessageId = (ulong)Volatile.Read(ref _nextFreeMessageId);

            // Case 1:
            // The client is already up-to-date with the message store, so we return no data.
            if (nextFreeMessageId <= firstMessageIdRequestedByClient)
            {
                return new MessageStoreResult<T>(firstMessageIdRequestedByClient, _emptyArraySegment, hasMoreData: false);
            }

            // look for the fragment containing the start of the data requested by the client
            ulong fragmentNum;
            int idxIntoFragmentsArray, idxIntoFragment;
            GetFragmentOffsets(firstMessageIdRequestedByClient, out fragmentNum, out idxIntoFragmentsArray, out idxIntoFragment);
            Fragment thisFragment = _fragments[idxIntoFragmentsArray];
            ulong firstMessageIdInThisFragment = GetMessageId(thisFragment.FragmentNum, offset: _offset);
            ulong firstMessageIdInNextFragment = firstMessageIdInThisFragment + _fragmentSize;

            // Case 2:
            // This fragment contains the first part of the data the client requested.
            if (firstMessageIdInThisFragment <= firstMessageIdRequestedByClient && firstMessageIdRequestedByClient < firstMessageIdInNextFragment)
            {
                int count = (int)(Math.Min(nextFreeMessageId, firstMessageIdInNextFragment) - firstMessageIdRequestedByClient);

                // Limit the number of messages the caller sees
                count = Math.Min(count, maxMessages);

                ArraySegment<T> retMessages = new ArraySegment<T>(thisFragment.Data, idxIntoFragment, count);

                return new MessageStoreResult<T>(firstMessageIdRequestedByClient, retMessages, hasMoreData: (nextFreeMessageId > firstMessageIdInNextFragment));
            }

            // Case 3:
            // The client has missed messages, so we need to send him the earliest fragment we have.
            while (true)
            {
                GetFragmentOffsets(nextFreeMessageId, out fragmentNum, out idxIntoFragmentsArray, out idxIntoFragment);
                Fragment tailFragment = _fragments[(idxIntoFragmentsArray + 1) % _fragments.Length];
                if (tailFragment.FragmentNum < fragmentNum)
                {
                    firstMessageIdInThisFragment = GetMessageId(tailFragment.FragmentNum, offset: _offset);
                    int count = Math.Min(maxMessages, tailFragment.Data.Length);
                    return new MessageStoreResult<T>(firstMessageIdInThisFragment, new ArraySegment<T>(tailFragment.Data, 0, count), hasMoreData: true);
                }
                nextFreeMessageId = (ulong)Volatile.Read(ref _nextFreeMessageId);
            }
        }

        private bool TryAddImpl(T message, out ulong newMessageId)
        {
            ulong nextFreeMessageId = (ulong)Volatile.Read(ref _nextFreeMessageId);

            // locate the fragment containing the next free id, which is where we should write
            ulong fragmentNum;
            int idxIntoFragmentsArray, idxIntoFragment;
            GetFragmentOffsets(nextFreeMessageId, out fragmentNum, out idxIntoFragmentsArray, out idxIntoFragment);
            Fragment fragment = _fragments[idxIntoFragmentsArray];

            if (fragment == null || fragment.FragmentNum < fragmentNum)
            {
                // the fragment is outdated (or non-existent) and must be replaced

                if (idxIntoFragment == 0)
                {
                    // this thread is responsible for creating the fragment
                    Fragment newFragment = new Fragment(fragmentNum, _fragmentSize);
                    newFragment.Data[0] = message;
                    Fragment existingFragment = Interlocked.CompareExchange(ref _fragments[idxIntoFragmentsArray], newFragment, fragment);
                    if (existingFragment == fragment)
                    {
                        newMessageId = GetMessageId(fragmentNum, offset: _offset);
                        return true;
                    }
                }

                // another thread is responsible for updating the fragment, so fall to bottom of method
            }
            else if (fragment.FragmentNum == fragmentNum)
            {
                // the fragment is valid, and we can just try writing into it until we reach the end of the fragment
                T[] fragmentData = fragment.Data;
                for (int i = idxIntoFragment; i < fragmentData.Length; i++)
                {
                    T originalMessage = Interlocked.CompareExchange(ref fragmentData[i], message, null);
                    if (originalMessage == null)
                    {
                        newMessageId = GetMessageId(fragmentNum, offset: (uint)i);
                        return true;
                    }
                }

                // another thread used the last open space in this fragment, so fall to bottom of method
            }

            // failure; caller will retry operation
            newMessageId = 0;
            return false;
        }

        private sealed class Fragment
        {
            public readonly ulong FragmentNum;
            public readonly T[] Data;

            public Fragment(ulong fragmentNum, uint fragmentSize)
            {
                FragmentNum = fragmentNum;
                Data = new T[fragmentSize];
            }
        }
    }
}
