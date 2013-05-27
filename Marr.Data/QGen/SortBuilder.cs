using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using Marr.Data.QGen.Dialects;

namespace Marr.Data.QGen
{
    /// <summary>
    /// This class is responsible for creating an "ORDER BY" clause.
    /// It uses chaining methods to provide a fluent interface.
    /// It also has some methods that coincide with Linq methods, to provide Linq compatibility.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SortBuilder<T> : IEnumerable<T>, ISortQueryBuilder
    {
        private string _constantOrderByClause;
        private QueryBuilder<T> _baseBuilder;
        private Dialect _dialect;
        private List<SortColumn<T>> _sortExpressions;
        private bool _useAltName;
        private TableCollection _tables;
        private IDataMapper _db;
        private WhereBuilder<T> _whereBuilder;

        public SortBuilder()
        {
            // Used only for unit testing with mock frameworks
        }

        public SortBuilder(QueryBuilder<T> baseBuilder, IDataMapper db, WhereBuilder<T> whereBuilder, Dialect dialect, TableCollection tables, bool useAltName)
        {
            _baseBuilder = baseBuilder;
            _db = db;
            _whereBuilder = whereBuilder;
            _dialect = dialect;
            _sortExpressions = new List<SortColumn<T>>();
            _useAltName = useAltName;
            _tables = tables;
        }

        #region - AndWhere / OrWhere -

        public virtual SortBuilder<T> OrWhere(Expression<Func<T, bool>> filterExpression)
        {
            var orWhere = new WhereBuilder<T>(_db.Command, _dialect, filterExpression, _tables, _useAltName, true);
            _whereBuilder.Append(orWhere, WhereAppendType.OR);
            return this;
        }

        public virtual SortBuilder<T> OrWhere(string whereClause)
        {
            var orWhere = new WhereBuilder<T>(whereClause, _useAltName);
            _whereBuilder.Append(orWhere, WhereAppendType.OR);
            return this;
        }

        public virtual SortBuilder<T> AndWhere(Expression<Func<T, bool>> filterExpression)
        {
            var andWhere = new WhereBuilder<T>(_db.Command, _dialect, filterExpression, _tables, _useAltName, true);
            _whereBuilder.Append(andWhere, WhereAppendType.AND);
            return this;
        }

        public virtual SortBuilder<T> AndWhere(string whereClause)
        {
            var andWhere = new WhereBuilder<T>(whereClause, _useAltName);
            _whereBuilder.Append(andWhere, WhereAppendType.AND);
            return this;
        }

        #endregion

        #region - Order -

        internal SortBuilder<T> Order(Type declaringType, string propertyName)
        {
            _sortExpressions.Add(new SortColumn<T>(declaringType, propertyName, SortDirection.Asc));
            return this;
        }

        internal SortBuilder<T> OrderByDescending(Type declaringType, string propertyName)
        {
            _sortExpressions.Add(new SortColumn<T>(declaringType, propertyName, SortDirection.Desc));
            return this;
        }
        
        public virtual SortBuilder<T> OrderBy(string orderByClause)
        {
            if (string.IsNullOrEmpty(orderByClause))
                throw new ArgumentNullException("orderByClause");

            if (!orderByClause.ToUpper().Contains("ORDER BY "))
            {
                orderByClause = orderByClause.Insert(0, " ORDER BY ");
            }

            _constantOrderByClause = orderByClause;
            return this;
        }

        public virtual SortBuilder<T> OrderBy(Expression<Func<T, object>> sortExpression)
        {
            _sortExpressions.Add(new SortColumn<T>(sortExpression, SortDirection.Asc));
            return this;
        }

        public virtual SortBuilder<T> OrderBy(Expression<Func<T, object>> sortExpression, SortDirection sortDirection)
        {
            _sortExpressions.Add(new SortColumn<T>(sortExpression, sortDirection));
            return this;
        }

        public virtual SortBuilder<T> OrderByDescending(Expression<Func<T, object>> sortExpression)
        {
            _sortExpressions.Add(new SortColumn<T>(sortExpression, SortDirection.Desc));
            return this;
        }

        public virtual SortBuilder<T> ThenBy(Expression<Func<T, object>> sortExpression)
        {
            _sortExpressions.Add(new SortColumn<T>(sortExpression, SortDirection.Asc));
            return this;
        }

        public virtual SortBuilder<T> ThenBy(Expression<Func<T, object>> sortExpression, SortDirection sortDirection)
        {
            _sortExpressions.Add(new SortColumn<T>(sortExpression, sortDirection));
            return this;
        }

        public virtual SortBuilder<T> ThenByDescending(Expression<Func<T, object>> sortExpression)
        {
            _sortExpressions.Add(new SortColumn<T>(sortExpression, SortDirection.Desc));
            return this;
        }

        #endregion

        #region - Paging -

        public virtual SortBuilder<T> Take(int count)
        {
            _baseBuilder.Take(count);
            return this;
        }

        public virtual SortBuilder<T> Skip(int count)
        {
            _baseBuilder.Skip(count);
            return this;
        }

        public virtual SortBuilder<T> Page(int pageNumber, int pageSize)
        {
            _baseBuilder.Page(pageNumber, pageSize);
            return this;
        }

        #endregion

        #region - GetRowCount -

        public virtual int GetRowCount()
        {
            return _baseBuilder.GetRowCount();
        }

        #endregion

        #region - ToList / ToString / BuildQuery -

        public virtual List<T> ToList()
        {
            return _baseBuilder.ToList();
        }

        public virtual string BuildQuery()
        {
            return _baseBuilder.BuildQuery();
        }

        public virtual string BuildQuery(bool useAltName)
        {
            if (!string.IsNullOrEmpty(_constantOrderByClause))
            {
                return _constantOrderByClause;
            }

            StringBuilder sb = new StringBuilder();

            foreach (var sort in _sortExpressions)
            {
                if (sb.Length > 0)
                    sb.Append(",");

                Table table = _tables.FindTable(sort.DeclaringType);

                if (table == null)
                {
                    string msg = string.Format("The property '{0} -> {1}' you are trying to reference in the 'ORDER BY' statement belongs to an entity that has not been joined in your query.  To reference this property, you must join the '{0}' entity using the Join method.",
                        sort.DeclaringType.Name,
                        sort.PropertyName);

                    throw new DataMappingException(msg);
                }

                string columnName = DataHelper.GetColumnName(sort.DeclaringType, sort.PropertyName, useAltName);

                if (!useAltName)
                    sb.Append(_dialect.CreateToken(string.Format("{0}.{1}", table.Alias, columnName)));

                else
                    sb.Append(_dialect.CreateToken(string.Format("{0}", columnName)));

                if (sort.Direction == SortDirection.Desc)
                    sb.Append(" DESC");
            }

            if (sb.Length > 0)
                sb.Insert(0, " ORDER BY ");

            return sb.ToString();
        }

        public override string ToString()
        {
            return BuildQuery(_useAltName);
        }

        #endregion

        #region - Implicit List<T> Operator -

        public static implicit operator List<T>(SortBuilder<T> builder)
        {
            return builder.ToList();
        }

        #endregion

        #region IEnumerable<T> Members

        public virtual IEnumerator<T> GetEnumerator()
        {
            var list = this.ToList();
            return list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
