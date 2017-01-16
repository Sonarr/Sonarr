using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Messaging.Commands
{
    public class CommandQueue : IProducerConsumerCollection<CommandModel>
    {
        private object Mutex = new object();

        private List<CommandModel> _items;

        public CommandQueue()
        {
            _items = new List<CommandModel>();
        }

        public IEnumerator<CommandModel> GetEnumerator()
        {
            List<CommandModel> copy = null;

            lock (Mutex)
            {
                copy = new List<CommandModel>(_items);
            }

            return copy.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            lock (Mutex)
            {
                ((ICollection)_items).CopyTo(array, index);
            }
        }

        public int Count => _items.Count;

        public object SyncRoot => Mutex;

        public bool IsSynchronized => true;

        public void CopyTo(CommandModel[] array, int index)
        {
            lock (Mutex)
            {
                _items.CopyTo(array, index);
            }
        }

        public bool TryAdd(CommandModel item)
        {
            Add(item);
            return true;
        }

        public bool TryTake(out CommandModel item)
        {
            bool rval = true;
            lock (Mutex)
            {
                if (_items.Count == 0)
                {
                    item = default(CommandModel);
                    rval = false;
                }

                else
                {
                    item = _items.Where(c => c.Status == CommandStatus.Queued)
                                 .OrderByDescending(c => c.Priority)
                                 .ThenBy(c => c.QueuedAt)
                                 .First();

                    _items.Remove(item);
                }
            }

            return rval;
        }

        public CommandModel[] ToArray()
        {
            CommandModel[] rval = null;

            lock (Mutex)
            {
                rval = _items.ToArray();
            }

            return rval;
        }

        public void Add(CommandModel item)
        {
            lock (Mutex)
            {
                _items.Add(item);
            }
        }
    }
}
