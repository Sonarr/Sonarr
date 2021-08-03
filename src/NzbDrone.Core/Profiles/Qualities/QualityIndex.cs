using System;

namespace NzbDrone.Core.Profiles.Qualities
{
    public class QualityIndex : IComparable, IComparable<QualityIndex>
    {
        public int Index { get; set; }
        public int GroupIndex { get; set; }

        public QualityIndex()
        {
            Index = 0;
            GroupIndex = 0;
        }

        public QualityIndex(int index)
        {
            Index = index;
            GroupIndex = 0;
        }

        public QualityIndex(int index, int groupIndex)
        {
            Index = index;
            GroupIndex = groupIndex;
        }

        public int CompareTo(object obj)
        {
            return CompareTo((QualityIndex)obj, true);
        }

        public int CompareTo(QualityIndex other)
        {
            return CompareTo(other, true);
        }

        public int CompareTo(QualityIndex right, bool respectGroupOrder)
        {
            if (right == null)
            {
                return 1;
            }

            var indexCompare = Index.CompareTo(right.Index);

            if (respectGroupOrder && indexCompare == 0)
            {
                return GroupIndex.CompareTo(right.GroupIndex);
            }

            return indexCompare;
        }
    }
}
