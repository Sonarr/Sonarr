using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marr.Data.Mapping;

namespace Marr.Data.QGen
{
    /// <summary>
    /// This class represents a table in a query.
    /// A table contains corresponding columns.
    /// </summary>
    public class Table
    {
        public Table(Type memberType)
            : this(memberType, JoinType.None)
        { }

        public Table(Type memberType, JoinType joinType)
        {
            EntityType = memberType;
            Name = memberType.GetTableName();
            JoinType = joinType;
            Columns = MapRepository.Instance.GetColumns(memberType);
        }

        public bool IsBaseTable
        {
            get
            {
                return Alias == "t0";
            }
        }

        public Type EntityType { get; private set; }
        public virtual string Name { get; set; }
        public JoinType JoinType { get; private set; }
        public virtual ColumnMapCollection Columns { get; private set; }
        public virtual string Alias { get; set; }
        public string JoinClause { get; set; }
    }

    public enum JoinType
    {
        None,
        Inner,
        Left,
        Right
    }
}
