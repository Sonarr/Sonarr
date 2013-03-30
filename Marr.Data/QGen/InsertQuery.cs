using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marr.Data.Mapping;
using System.Data.Common;
using Marr.Data.QGen.Dialects;

namespace Marr.Data.QGen
{
    /// <summary>
    /// This class creates an insert query.
    /// </summary>
    public class InsertQuery : IQuery
    {
        protected Dialect Dialect { get; set; }
        protected string Target { get; set; }
        protected ColumnMapCollection Columns { get; set; }
        protected DbCommand Command { get; set; }

        public InsertQuery(Dialect dialect, ColumnMapCollection columns, DbCommand command, string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                throw new DataMappingException("A target table must be passed in or set in a TableAttribute.");
            }
            Dialect = dialect;
            Target = target;
            Columns = columns;
            Command = command;
        }

        public virtual string Generate()
        {
            StringBuilder sql = new StringBuilder();
            StringBuilder values = new StringBuilder(") VALUES (");

            sql.AppendFormat("INSERT INTO {0} (", Dialect.CreateToken(Target));

            int sqlStartIndex = sql.Length;
            int valuesStartIndex = values.Length;

            foreach (DbParameter p in Command.Parameters)
            {
                var c = Columns.GetByColumnName(p.ParameterName);

                if (c == null)
                    break; // All insert columns have been added

                if (sql.Length > sqlStartIndex)
                    sql.Append(",");

                if (values.Length > valuesStartIndex)
                    values.Append(",");

                if (!c.ColumnInfo.IsAutoIncrement)
                {
                    sql.AppendFormat(Dialect.CreateToken(c.ColumnInfo.Name));
                    values.AppendFormat("{0}{1}", Command.ParameterPrefix(), p.ParameterName);
                }
            }

            values.Append(")");

            sql.Append(values);

            return sql.ToString();
        }
    }
}
