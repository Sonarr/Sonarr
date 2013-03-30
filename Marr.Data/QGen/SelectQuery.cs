using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marr.Data;
using Marr.Data.Mapping;
using System.Data.Common;
using Marr.Data.QGen.Dialects;

namespace Marr.Data.QGen
{
    /// <summary>
    /// This class is responsible for creating a select query.
    /// </summary>
    public class SelectQuery : IQuery
    {
        public Dialect Dialect { get; set; }
        public string WhereClause { get; set; }
        public ISortQueryBuilder OrderBy { get; set; }
        public TableCollection Tables { get; set; }
        public bool UseAltName;

        public SelectQuery(Dialect dialect, TableCollection tables, string whereClause, ISortQueryBuilder orderBy, bool useAltName)
        {
            Dialect = dialect;
            Tables = tables;
            WhereClause = whereClause;
            OrderBy = orderBy;
            UseAltName = useAltName;
        }

        public bool IsView
        {
            get
            {
                return Tables[0] is View;
            }
        }

        public bool IsJoin
        {
            get
            {
                return Tables.Count > 1;
            }
        }

        public virtual string Generate()
        {
            StringBuilder sql = new StringBuilder();

            BuildSelectClause(sql);
            BuildFromClause(sql);
            BuildJoinClauses(sql);
            BuildWhereClause(sql);
            BuildOrderClause(sql);

            return sql.ToString();
        }

        public void BuildSelectClause(StringBuilder sql)
        {
            sql.Append("SELECT ");

            int startIndex = sql.Length;

            // COLUMNS
            foreach (Table join in Tables)
            {
                for (int i = 0; i < join.Columns.Count; i++)
                {
                    var c = join.Columns[i];

                    if (sql.Length > startIndex)
                        sql.Append(",");

                    if (join is View)
                    {
                        string token = string.Concat(join.Alias, ".", NameOrAltName(c.ColumnInfo));
                        sql.Append(Dialect.CreateToken(token));
                    }
                    else
                    {
                        string token = string.Concat(join.Alias, ".", c.ColumnInfo.Name);
                        sql.Append(Dialect.CreateToken(token));

                        if (UseAltName && c.ColumnInfo.AltName != null && c.ColumnInfo.AltName != c.ColumnInfo.Name)
                        {
                            string altName = c.ColumnInfo.AltName;
                            sql.AppendFormat(" AS {0}", altName);
                        }
                    }
                }
            }
        }

        public string NameOrAltName(IColumnInfo columnInfo)
        {
            if (UseAltName && columnInfo.AltName != null && columnInfo.AltName != columnInfo.Name)
            {
                return columnInfo.AltName;
            }
            else
            {
                return columnInfo.Name;
            }
        }

        public void BuildFromClause(StringBuilder sql)
        {
            // BASE TABLE
            Table baseTable = Tables[0];
            sql.AppendFormat(" FROM {0} {1} ", Dialect.CreateToken(baseTable.Name), Dialect.CreateToken(baseTable.Alias));
        }

        public void BuildJoinClauses(StringBuilder sql)
        {
            // JOINS
            for (int i = 1; i < Tables.Count; i++)
            {
                if (Tables[i].JoinType != JoinType.None)
                {
                    sql.AppendFormat("{0} {1} {2} {3} ",
                        TranslateJoin(Tables[i].JoinType),
                        Dialect.CreateToken(Tables[i].Name),
                        Dialect.CreateToken(Tables[i].Alias),
                        Tables[i].JoinClause);
                }
            }
        }

        public void BuildWhereClause(StringBuilder sql)
        {
            sql.Append(WhereClause);
        }

        public void BuildOrderClause(StringBuilder sql)
        {
            sql.Append(OrderBy.ToString());
        }       

        private string TranslateJoin(JoinType join)
        {
            switch (join)
            {
                case JoinType.Inner:
                    return "INNER JOIN";
                case JoinType.Left:
                    return "LEFT JOIN";
                case JoinType.Right:
                    return "RIGHT JOIN";
                default:
                    return string.Empty;
            }
        }
    }
}
