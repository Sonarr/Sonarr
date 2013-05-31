using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marr.Data.Mapping;
using Marr.Data.QGen.Dialects;

namespace Marr.Data.QGen
{
    /// <summary>
    /// Decorates the SelectQuery by wrapping it in a paging query.
    /// </summary>
    public class SqlitePagingQueryDecorator : IQuery
    {
        private SelectQuery _innerQuery;
        private int _skip;
        private int _take;

        public SqlitePagingQueryDecorator(SelectQuery innerQuery, int skip, int take)
        {
            if (string.IsNullOrEmpty(innerQuery.OrderBy.ToString()))
            {
                throw new DataMappingException("A paged query must specify an order by clause.");
            }

            _innerQuery = innerQuery;
            _skip = skip;
            _take = take;
        }

        public string Generate()
        {
            if (_innerQuery.IsView || _innerQuery.IsJoin)
            {
                return ComplexPaging();
            }
            else
            {
                return SimplePaging();
            }
        }

        private string SimplePaging()
        {
            // Create paged query
            StringBuilder sql = new StringBuilder();

            _innerQuery.BuildSelectClause(sql);
            _innerQuery.BuildFromClause(sql);
            _innerQuery.BuildJoinClauses(sql);
            _innerQuery.BuildWhereClause(sql);
            _innerQuery.BuildOrderClause(sql);
            sql.AppendLine(String.Format(" LIMIT {0},{1}", _skip, _take));

            return sql.ToString();
        }

        private string ComplexPaging()
        {
            var baseTable = _innerQuery.Tables.First();


            StringBuilder sql = new StringBuilder();

            _innerQuery.BuildSelectClause(sql);
            sql.Append(" FROM (");
            BuildSimpleInnerSelect(sql);
            _innerQuery.BuildFromClause(sql);
            _innerQuery.BuildJoinClauses(sql);
            _innerQuery.BuildWhereClause(sql);
            BuildGroupBy(sql);
            BuildOrderBy(sql);
            sql.AppendFormat(" LIMIT {0},{1}", _skip, _take);
            sql.AppendFormat(") AS {0} ", _innerQuery.Dialect.CreateToken(baseTable.Alias));

            _innerQuery.BuildJoinClauses(sql);

            return sql.ToString();
        }

        public void BuildSelectClause(StringBuilder sql)
        {
            List<string> appended = new List<string>();

            sql.Append("SELECT ");

            int startIndex = sql.Length;

            // COLUMNS
            foreach (Table join in _innerQuery.Tables)
            {
                for (int i = 0; i < join.Columns.Count; i++)
                {
                    var c = join.Columns[i];

                    if (sql.Length > startIndex && sql[sql.Length - 1] != ',')
                        sql.Append(",");

                    if (join is View)
                    {
                        string token = _innerQuery.Dialect.CreateToken(string.Concat(join.Alias, ".", _innerQuery.NameOrAltName(c.ColumnInfo)));
                        if (appended.Contains(token))
                            continue;

                        sql.Append(token);
                        appended.Add(token);
                    }
                    else
                    {
                        string token = string.Concat(join.Alias, ".", c.ColumnInfo.Name);
                        if (appended.Contains(token))
                            continue;

                        sql.Append(_innerQuery.Dialect.CreateToken(token));

                        if (_innerQuery.UseAltName && c.ColumnInfo.AltName != null && c.ColumnInfo.AltName != c.ColumnInfo.Name)
                        {
                            string altName = c.ColumnInfo.AltName;
                            sql.AppendFormat(" AS {0}", altName);
                        }
                    }
                }
            }
        }

        private void BuildSimpleInnerSelect(StringBuilder sql)
        {
            sql.Append("SELECT ");
            int startIndex = sql.Length;

            // COLUMNS
            var join = _innerQuery.Tables.First();

            for (int i = 0; i < join.Columns.Count; i++)
            {
                var c = join.Columns[i];

                if (sql.Length > startIndex)
                    sql.Append(",");

                string token = string.Concat(join.Alias, ".", c.ColumnInfo.Name);
                sql.Append(_innerQuery.Dialect.CreateToken(token));
            }

        }

        private void BuildOrderBy(StringBuilder sql)
        {
            sql.Append(_innerQuery.OrderBy.BuildQuery(false));
        }

        private void BuildGroupBy(StringBuilder sql)
        {
            var baseTable = _innerQuery.Tables.First();
            var primaryKeyColumn = baseTable.Columns.Single(c => c.ColumnInfo.IsPrimaryKey);

            string token = _innerQuery.Dialect.CreateToken(string.Concat(baseTable.Alias, ".", primaryKeyColumn.ColumnInfo.Name));
            sql.AppendFormat(" GROUP BY {0}", token);
        }
    }
}
