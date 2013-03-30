using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marr.Data.Mapping;
using System.Data.Common;

namespace Marr.Data
{
    public class GroupingKeyCollection : IEnumerable<ColumnMap>
    {
        public GroupingKeyCollection()
        {
            PrimaryKeys = new ColumnMapCollection();
            ParentPrimaryKeys = new ColumnMapCollection();
        }

        public ColumnMapCollection PrimaryKeys { get; private set; }
        public ColumnMapCollection ParentPrimaryKeys { get; private set; }

        public int Count
        {
            get
            {
                return PrimaryKeys.Count + ParentPrimaryKeys.Count;
            }
        }

        /// <summary>
        /// Gets the PK values that define this entity in the graph.
        /// </summary>
        internal string GroupingKey { get; private set; }

        /// <summary>
        /// Returns a concatented string containing the primary key values of the current record.
        /// </summary>
        /// <param name="reader">The open data reader.</param>
        /// <returns>Returns the primary key value(s) as a string.</returns>
        internal KeyGroupInfo CreateGroupingKey(DbDataReader reader)
        {
            StringBuilder pkValues = new StringBuilder();
            bool hasNullValue = false;

            foreach (ColumnMap pkColumn in this)
            {
                object pkValue = reader[pkColumn.ColumnInfo.GetColumName(true)];

                if (pkValue == DBNull.Value)
                    hasNullValue = true;

                pkValues.Append(pkValue.ToString());
            }

            GroupingKey = pkValues.ToString();

            return new KeyGroupInfo(GroupingKey, hasNullValue);
        }

        #region IEnumerable<ColumnMap> Members

        public IEnumerator<ColumnMap> GetEnumerator()
        {
            foreach (ColumnMap map in ParentPrimaryKeys)
            {
                yield return map;
            }

            foreach (ColumnMap map in PrimaryKeys)
            {
                yield return map;
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}
