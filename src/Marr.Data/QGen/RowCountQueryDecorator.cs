using System.Text;

namespace Marr.Data.QGen
{
    public class RowCountQueryDecorator : IQuery
    {
        private SelectQuery _innerQuery;

        public RowCountQueryDecorator(SelectQuery innerQuery)
        {
            _innerQuery = innerQuery;
        }
        
        public string Generate()
        {
            // Decide which type of paging query to create
            if (_innerQuery.IsView || _innerQuery.IsJoin)
            {
                return ComplexRowCount();
            }
            return SimpleRowCount();
        }

        /// <summary>
        /// Generates a row count query for a multiple table joined query (groups by the parent entity).
        /// </summary>
        /// <returns></returns>
        private string ComplexRowCount()
        {
            // Create paged query
            StringBuilder sql = new StringBuilder();

            sql.AppendLine("WITH GroupCTE AS (");
            sql.Append("SELECT ").AppendLine(BuildBaseTablePKColumns());
            BuildGroupColumn(sql);
            _innerQuery.BuildFromClause(sql);
            _innerQuery.BuildJoinClauses(sql);
            _innerQuery.BuildWhereClause(sql);
            sql.AppendLine(")");
            BuildSelectCountClause(sql);
            sql.AppendLine("FROM GroupCTE");
            sql.AppendLine("WHERE GroupRow = 1");

            return sql.ToString();
        }

        /// <summary>
        /// Generates a row count query for a single table query (no joins).
        /// </summary>
        /// <returns></returns>
        private string SimpleRowCount()
        {
            StringBuilder sql = new StringBuilder();

            BuildSelectCountClause(sql);
            _innerQuery.BuildFromClause(sql);
            _innerQuery.BuildJoinClauses(sql);
            _innerQuery.BuildWhereClause(sql);

            return sql.ToString();
        }

        private void BuildGroupColumn(StringBuilder sql)
        {
            string baseTablePKColumns = BuildBaseTablePKColumns();
            sql.AppendFormat(", ROW_NUMBER() OVER (PARTITION BY {0} ORDER BY {1}) As GroupRow ", baseTablePKColumns, baseTablePKColumns);
        }

        private string BuildBaseTablePKColumns()
        {
            Table baseTable = GetBaseTable();

            StringBuilder sb = new StringBuilder();
            foreach (var col in baseTable.Columns.PrimaryKeys)
            {
                if (sb.Length > 0)
                    sb.AppendLine(", ");

                string colName = _innerQuery.IsView ?
                    _innerQuery.NameOrAltName(col.ColumnInfo) :
                    col.ColumnInfo.Name;

                sb.AppendFormat(_innerQuery.Dialect.CreateToken(string.Concat(baseTable.Alias, ".", colName)));
            }

            return sb.ToString();
        }

        private void BuildSelectCountClause(StringBuilder sql)
        {
            sql.AppendLine("SELECT COUNT(*)");
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
    }
}

/*
WITH GroupCTE AS 
(
	SELECT [t0].[ID],[t0].[OrderName],[t1].[ID] AS OrderItemID,[t1].[OrderID],[t1].[ItemDescription],[t1].[Price], 
	ROW_NUMBER() OVER (PARTITION BY [t0].[ID]  ORDER BY [t0].[OrderName]) As GroupRow  
	FROM [Order] [t0] 
	LEFT JOIN [OrderItem] [t1] ON (([t0].[ID] = [t1].[OrderID])) 
	--WHERE (([t0].[OrderName] = @P0))
)
SELECT * FROM GroupCTE
WHERE GroupRow = 1
*/