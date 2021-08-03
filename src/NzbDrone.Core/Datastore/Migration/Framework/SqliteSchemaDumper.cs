using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator.Model;
using FluentMigrator.Runner.Processors.SQLite;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    // Modeled after the FluentMigrator SchemaDumper class.
    // The original implementation had bad support for escaped identifiers, amongst other things.
    public class SqliteSchemaDumper
    {
        public SqliteSchemaDumper(SQLiteProcessor processor)
        {
            Processor = processor;
        }

        public SQLiteProcessor Processor { get; set; }

        protected internal virtual TableDefinition ReadTableSchema(string sqlSchema)
        {
            var reader = new SqliteSyntaxReader(sqlSchema);

            var result = ParseCreateTableStatement(reader);

            return result;
        }

        protected internal virtual IndexDefinition ReadIndexSchema(string sqlSchema)
        {
            var reader = new SqliteSyntaxReader(sqlSchema);

            var result = ParseCreateIndexStatement(reader);

            return result;
        }

        protected virtual TableDefinition ParseCreateTableStatement(SqliteSyntaxReader reader)
        {
            var table = new TableDefinition();

            while (reader.Read() != SqliteSyntaxReader.TokenType.StringToken || reader.ValueToUpper != "TABLE")
            {
            }

            if (reader.Read() == SqliteSyntaxReader.TokenType.StringToken && reader.ValueToUpper == "IF")
            {
                reader.Read(); // NOT
                reader.Read(); // EXISTS
            }
            else
            {
                reader.Rollback();
            }

            table.Name = ParseIdentifier(reader);

            // Find Column List
            reader.SkipTillToken(SqliteSyntaxReader.TokenType.ListStart);

            // Split the list.
            var list = reader.ReadList();

            foreach (var columnReader in list)
            {
                columnReader.SkipWhitespace();

                if (columnReader.Read() == SqliteSyntaxReader.TokenType.StringToken)
                {
                    if (columnReader.ValueToUpper == "PRIMARY")
                    {
                        columnReader.SkipTillToken(SqliteSyntaxReader.TokenType.ListStart);
                        if (columnReader.Read() == SqliteSyntaxReader.TokenType.Identifier)
                        {
                            var pk = table.Columns.First(v => v.Name == columnReader.Value);
                            pk.IsPrimaryKey = true;
                            pk.IsNullable = true;
                            pk.IsUnique = true;
                            if (columnReader.Buffer.ToUpperInvariant().Contains("AUTOINCREMENT"))
                            {
                                pk.IsIdentity = true;
                            }

                            continue;
                        }
                    }

                    if (columnReader.ValueToUpper == "CONSTRAINT" ||
                        columnReader.ValueToUpper == "PRIMARY" || columnReader.ValueToUpper == "UNIQUE" ||
                        columnReader.ValueToUpper == "CHECK" || columnReader.ValueToUpper == "FOREIGN")
                    {
                        continue;
                    }
                }

                columnReader.Rollback();

                var column = ParseColumnDefinition(columnReader);
                column.TableName = table.Name;
                table.Columns.Add(column);
            }

            return table;
        }

        protected virtual ColumnDefinition ParseColumnDefinition(SqliteSyntaxReader reader)
        {
            var column = new ColumnDefinition();

            column.Name = ParseIdentifier(reader);

            reader.TrimBuffer();

            reader.Read();
            if (reader.Type != SqliteSyntaxReader.TokenType.End)
            {
                column.Type = GetDbType(reader.Value);

                var upper = reader.Buffer.ToUpperInvariant();
                column.IsPrimaryKey = upper.Contains("PRIMARY KEY");
                column.IsIdentity = upper.Contains("AUTOINCREMENT");
                column.IsNullable = !upper.Contains("NOT NULL") && !upper.Contains("PRIMARY KEY");
                column.IsUnique = upper.Contains("UNIQUE") || upper.Contains("PRIMARY KEY");
            }

            return column;
        }

        protected virtual IndexDefinition ParseCreateIndexStatement(SqliteSyntaxReader reader)
        {
            var index = new IndexDefinition();

            reader.Read();

            reader.Read();
            index.IsUnique = reader.ValueToUpper == "UNIQUE";

            while (reader.ValueToUpper != "INDEX")
            {
                reader.Read();
            }

            if (reader.Read() == SqliteSyntaxReader.TokenType.StringToken && reader.ValueToUpper == "IF")
            {
                reader.Read(); // NOT
                reader.Read(); // EXISTS
            }
            else
            {
                reader.Rollback();
            }

            index.Name = ParseIdentifier(reader);

            reader.Read(); // ON

            index.TableName = ParseIdentifier(reader);

            // Find Column List
            reader.SkipTillToken(SqliteSyntaxReader.TokenType.ListStart);

            // Split the list.
            var list = reader.ReadList();

            foreach (var columnReader in list)
            {
                var column = new IndexColumnDefinition();
                column.Name = ParseIdentifier(columnReader);

                while (columnReader.Read() == SqliteSyntaxReader.TokenType.StringToken)
                {
                    if (columnReader.ValueToUpper == "COLLATE")
                    {
                        columnReader.Read(); // Skip Collation name
                    }
                    else if (columnReader.ValueToUpper == "DESC")
                    {
                        column.Direction = Direction.Descending;
                    }
                }

                index.Columns.Add(column);
            }

            return index;
        }

        protected virtual string ParseIdentifier(SqliteSyntaxReader reader)
        {
            reader.Read();

            if (reader.Type != SqliteSyntaxReader.TokenType.Identifier &&
                reader.Type != SqliteSyntaxReader.TokenType.StringToken &&
                reader.Type != SqliteSyntaxReader.TokenType.StringLiteral)
            {
                throw reader.CreateSyntaxException("Expected Identifier but found {0}", reader.Type);
            }

            return reader.Value;
        }

        public virtual IList<TableDefinition> ReadDbSchema()
        {
            IList<TableDefinition> tables = ReadTables();
            foreach (var table in tables)
            {
                table.Indexes = ReadIndexes(table.SchemaName, table.Name);

                //table.ForeignKeys = ReadForeignKeys(table.SchemaName, table.Name);
            }

            return tables;
        }

        protected virtual DataSet Read(string template, params object[] args)
        {
            return Processor.Read(template, args);
        }

        protected virtual IList<TableDefinition> ReadTables()
        {
            const string sqlCommand = @"SELECT name, sql FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name;";
            var dtTable = Read(sqlCommand).Tables[0];

            var tableDefinitionList = new List<TableDefinition>();

            foreach (DataRow dr in dtTable.Rows)
            {
                var sql = dr["sql"].ToString();
                var table = ReadTableSchema(sql);

                tableDefinitionList.Add(table);
            }

            return tableDefinitionList;
        }

        /// <summary>
        /// Get DbType from string type definition
        /// </summary>
        /// <param name="typeNum"></param>
        /// <returns></returns>
        public static DbType? GetDbType(string typeNum)
        {
            switch (typeNum.ToUpper())
            {
                case "BLOB":
                    return DbType.Binary;
                case "INTEGER":
                    return DbType.Int64;
                case "NUMERIC":
                    return DbType.Double;
                case "TEXT":
                    return DbType.String;
                case "DATETIME":
                    return DbType.DateTime;
                case "UNIQUEIDENTIFIER":
                    return DbType.Guid;
                default:
                    return null;
            }
        }

        protected virtual IList<IndexDefinition> ReadIndexes(string schemaName, string tableName)
        {
            var sqlCommand = string.Format(@"SELECT type, name, sql FROM sqlite_master WHERE tbl_name = '{0}' AND type = 'index' AND name NOT LIKE 'sqlite_auto%';", tableName);
            DataTable table = Read(sqlCommand).Tables[0];

            IList<IndexDefinition> indexes = new List<IndexDefinition>();

            foreach (DataRow dr in table.Rows)
            {
                var sql = dr["sql"].ToString();
                var index = ReadIndexSchema(sql);
                indexes.Add(index);
            }

            return indexes;
        }
    }
}
