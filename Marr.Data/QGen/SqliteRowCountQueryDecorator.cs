using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marr.Data.QGen
{
    public class SqliteRowCountQueryDecorator : IQuery
    {
        private SelectQuery _innerQuery;

        public SqliteRowCountQueryDecorator(SelectQuery innerQuery)
        {
            _innerQuery = innerQuery;
        }
        
        public string Generate()
        {
            StringBuilder sql = new StringBuilder();

            BuildSelectCountClause(sql);
            _innerQuery.BuildFromClause(sql);
            _innerQuery.BuildJoinClauses(sql);
            _innerQuery.BuildWhereClause(sql);

            return sql.ToString();
        }

        private void BuildSelectCountClause(StringBuilder sql)
        {
            sql.AppendLine("SELECT COUNT(*)");
        }
    }
}