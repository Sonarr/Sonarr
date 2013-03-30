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
    public class PagingQueryDecorator : IQuery
    {
        private SelectQuery _innerQuery;
        private int _firstRow;
        private int _lastRow;

        public PagingQueryDecorator(SelectQuery innerQuery, int skip, int take)
        {
            if (string.IsNullOrEmpty(innerQuery.OrderBy.ToString()))
            {
                throw new DataMappingException("A paged query must specify an order by clause.");
            }

            _innerQuery = innerQuery;
            _firstRow = skip + 1;
            _lastRow = skip + take;
        }

        public string Generate()
        {
            // Decide which type of paging query to create
            
            if (_innerQuery.IsView || _innerQuery.IsJoin)
            {
                return ComplexPaging();
            }
            else
            {
                return SimplePaging();
            }
        }

        /// <summary>
        /// Generates a query that pages a simple inner query.
        /// </summary>
        /// <returns></returns>
        private string SimplePaging()
        {
            // Create paged query
            StringBuilder sql = new StringBuilder();

            sql.AppendLine("WITH RowNumCTE AS");
            sql.AppendLine("(");
            _innerQuery.BuildSelectClause(sql);
            BuildRowNumberColumn(sql);
            _innerQuery.BuildFromClause(sql);
            _innerQuery.BuildJoinClauses(sql);
            _innerQuery.BuildWhereClause(sql);
            sql.AppendLine(")");
            BuildSimpleOuterSelect(sql);

            return sql.ToString();
        }
        
        /// <summary>
        /// Generates a query that pages a view or joined inner query.
        /// </summary>
        /// <returns></returns>
        private string ComplexPaging()
        {
            // Create paged query
            StringBuilder sql = new StringBuilder();

            sql.AppendLine("WITH GroupCTE AS (");
            BuildSelectClause(sql);
            BuildGroupColumn(sql);
            _innerQuery.BuildFromClause(sql);
            _innerQuery.BuildJoinClauses(sql);
            _innerQuery.BuildWhereClause(sql);
            sql.AppendLine("),");
            sql.AppendLine("RowNumCTE AS (");
            sql.AppendLine("SELECT *");
            BuildRowNumberColumn(sql);
            sql.AppendLine("FROM GroupCTE");
            sql.AppendLine("WHERE GroupRow = 1");
            sql.AppendLine(")");
            _innerQuery.BuildSelectClause(sql);
            _innerQuery.BuildFromClause(sql);
            _innerQuery.BuildJoinClauses(sql);
            BuildJoinBackToCTE(sql);
            sql.AppendFormat("WHERE RowNumber BETWEEN {0} AND {1}", _firstRow, _lastRow);

            return sql.ToString();
        }

        private void BuildJoinBackToCTE(StringBuilder sql)
        {
            Table baseTable = GetBaseTable();
            sql.AppendLine("INNER JOIN RowNumCTE cte");
            int pksAdded = 0;
            foreach (var pk in baseTable.Columns.PrimaryKeys)
            {
                if (pksAdded > 0)
                    sql.Append(" AND ");

                string cteQueryPkName = _innerQuery.NameOrAltName(pk.ColumnInfo);
                string outerQueryPkName = _innerQuery.IsJoin ? pk.ColumnInfo.Name : _innerQuery.NameOrAltName(pk.ColumnInfo);
                sql.AppendFormat("ON cte.{0} = {1} ", cteQueryPkName, _innerQuery.Dialect.CreateToken(string.Concat("t0", ".", outerQueryPkName)));
                pksAdded++;
            }
            sql.AppendLine();
        }

        private void BuildSimpleOuterSelect(StringBuilder sql)
        {
            sql.Append("SELECT ");
            int startIndex = sql.Length;

            // COLUMNS
            foreach (Table join in _innerQuery.Tables)
            {
                for (int i = 0; i < join.Columns.Count; i++)
                {
                    var c = join.Columns[i];

                    if (sql.Length > startIndex)
                        sql.Append(",");

                    string token = _innerQuery.NameOrAltName(c.ColumnInfo);
                    sql.Append(_innerQuery.Dialect.CreateToken(token));
                }
            }

            sql.AppendLine("FROM RowNumCTE");
            sql.AppendFormat("WHERE RowNumber BETWEEN {0} AND {1}", _firstRow, _lastRow).AppendLine();
            sql.AppendLine("ORDER BY RowNumber ASC;");
        }

        private void BuildGroupColumn(StringBuilder sql)
        {
            bool isView = _innerQuery.IsView;
            sql.AppendFormat(", ROW_NUMBER() OVER (PARTITION BY {0} {1}) As GroupRow ", BuildBaseTablePKColumns(isView), _innerQuery.OrderBy.BuildQuery(isView));
        }

        private string BuildBaseTablePKColumns(bool useAltName = true)
        {
            Table baseTable = GetBaseTable();

            StringBuilder sb = new StringBuilder();
            foreach (var col in baseTable.Columns.PrimaryKeys)
            {
                if (sb.Length > 0)
                    sb.AppendLine(", ");

                string columnName = useAltName ?
                    _innerQuery.NameOrAltName(col.ColumnInfo) :
                    col.ColumnInfo.Name;

                sb.AppendFormat(_innerQuery.Dialect.CreateToken(string.Concat(baseTable.Alias, ".", columnName)));
            }

            return sb.ToString();
        }

        private void BuildRowNumberColumn(StringBuilder sql)
        {
            string orderBy = _innerQuery.OrderBy.ToString();
            // Remove table prefixes from order columns
            foreach (Table t in _innerQuery.Tables)
            {
                orderBy = orderBy.Replace(string.Format("[{0}].", t.Alias), "");
            }
            
            sql.AppendFormat(", ROW_NUMBER() OVER ({0}) As RowNumber ", orderBy);
        }

        private Table GetBaseTable()
        {
            Table baseTable = null;
            if (_innerQuery.Tables[0] is View)
            {
                baseTable = (_innerQuery.Tables[0] as View).Tables[0];
            }
            else
            {
                baseTable = _innerQuery.Tables[0];
            }
            return baseTable;
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

    }
}
