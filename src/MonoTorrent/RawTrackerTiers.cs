using System;
using System.Collections;
using System.Collections.Generic;

using MonoTorrent.BEncoding;

namespace MonoTorrent
{
    public class RawTrackerTiers : IList<RawTrackerTier>
    {
        BEncodedList Tiers {
            get; set;
        }

        public RawTrackerTiers ()
            : this (new BEncodedList ())
        {
        }

        public RawTrackerTiers (BEncodedList tiers)
        {
            Tiers = tiers;
        }

        public int IndexOf (RawTrackerTier item)
        {
            if (item != null) {
                for (int i = 0; i < Tiers.Count; i++)
                    if (item.Tier == Tiers [i])
                        return i;
            }
            return -1;
        }

        public void Insert (int index, RawTrackerTier item)
        {
            Tiers.Insert (index, item.Tier);
        }

        public void RemoveAt (int index)
        {
            Tiers.RemoveAt (index);
        }

        public RawTrackerTier this[int index] {
            get { return new RawTrackerTier ((BEncodedList) Tiers [index]); }
            set { Tiers [index] = value.Tier; }
        }

        public void Add (RawTrackerTier item)
        {
            Tiers.Add (item.Tier);
        }

        public void AddRange (IEnumerable<RawTrackerTier> tiers)
        {
            foreach (var v in tiers)
                Add (v);
        }

        public void Clear ()
        {
            Tiers.Clear ();
        }

        public bool Contains (RawTrackerTier item)
        {
            return IndexOf (item) != -1;
        }

        public void CopyTo (RawTrackerTier[] array, int arrayIndex)
        {
            foreach (var v in this)
                array [arrayIndex ++] = v;
        }

        public bool Remove (RawTrackerTier item)
        {
            int index = IndexOf (item);
            if (index != -1)
                RemoveAt (index);

            return index != -1;
        }

        public int Count {
            get { return Tiers.Count; }
        }

        public bool IsReadOnly {
            get { return Tiers.IsReadOnly; }
        }

        public IEnumerator<RawTrackerTier> GetEnumerator ()
        {
            foreach (var v in Tiers)
                yield return new RawTrackerTier ((BEncodedList) v);
        }

        IEnumerator IEnumerable.GetEnumerator ()
        {
            return GetEnumerator ();
        }
    }
}
