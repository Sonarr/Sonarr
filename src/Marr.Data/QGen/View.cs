using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Marr.Data.Mapping;

namespace Marr.Data.QGen
{
    /// <summary>
    /// This class represents a View.  A view can hold multiple tables (and their columns).
    /// </summary>
    public class View : Table, IEnumerable<Table>
    {
        private string _viewName;
        private Table[] _tables;
        private ColumnMapCollection _columns;

        public View(string viewName, Table[] tables)
            : base(tables[0].EntityType, JoinType.None)
        {
            _viewName = viewName;
            _tables = tables;
        }

        public Table[] Tables
        {
            get { return _tables; }
        }
        
        public override string Name
        {
            get
            {
                return _viewName;
            }
            set
            {
                _viewName = value;
            }
        }

        public override string Alias
        {
            get
            {
                return base.Alias;
            }
            set
            {
                base.Alias = value;

                // Sync view tables
                foreach (Table table in _tables)
                {
                    table.Alias = value;
                }
            }
        }

        /// <summary>
        /// Gets all the columns from all the tables included in the view.
        /// </summary>
        public override ColumnMapCollection Columns
        {
            get
            {
                if (_columns == null)
                {
                    var allColumns = _tables.SelectMany(t => t.Columns);
                    _columns = new ColumnMapCollection();
                    _columns.AddRange(allColumns);
                }

                return _columns;
            }
        }

        public IEnumerator<Table> GetEnumerator()
        {
            foreach (Table table in _tables)
            {
                yield return table;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
