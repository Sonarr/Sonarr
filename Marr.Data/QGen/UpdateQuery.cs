using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using Marr.Data.Mapping;
using Marr.Data.QGen.Dialects;

namespace Marr.Data.QGen
{
    public class UpdateQuery : IQuery
    {
        protected Dialect Dialect { get; set; }
        protected string Target { get; set; }
        protected ColumnMapCollection Columns { get; set; }
        protected DbCommand Command { get; set; }
        protected string WhereClause { get; set; }

        public UpdateQuery(Dialect dialect, ColumnMapCollection columns, DbCommand command, string target, string whereClause)
        {
            Dialect = dialect;
            Target = target;
            Columns = columns;
            Command = command;
            WhereClause = whereClause;
        }

        public string Generate()
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendFormat("UPDATE {0} SET ", Dialect.CreateToken(Target));

            int startIndex = sql.Length;

            foreach (DbParameter p in Command.Parameters)
            {
                var c = Columns.GetByColumnName(p.ParameterName);

                if (c == null)
                    break; // All SET columns have been added

                if (sql.Length > startIndex)
                    sql.Append(",");

                if (!c.ColumnInfo.IsAutoIncrement)
                {
                    sql.AppendFormat("{0}={1}{2}", Dialect.CreateToken(c.ColumnInfo.Name), Command.ParameterPrefix(), p.ParameterName);
                }
            }

            sql.AppendFormat(" {0}", WhereClause);

            return sql.ToString();
        }


    }
}
