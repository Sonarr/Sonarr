using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public interface ISQLiteMigrationHelper
    {
        Dictionary<String, SQLiteMigrationHelper.SQLiteColumn> GetColumns(string tableName);
        void CreateTable(string tableName, IEnumerable<SQLiteMigrationHelper.SQLiteColumn> values, IEnumerable<SQLiteMigrationHelper.SQLiteIndex> indexes);
        void CopyData(string sourceTable, string destinationTable, IEnumerable<SQLiteMigrationHelper.SQLiteColumn> columns);
        void DropTable(string tableName);
        void RenameTable(string tableName, string newName);
        SQLiteTransaction BeginTransaction();
        List<SQLiteMigrationHelper.SQLiteIndex> GetIndexes(string tableName);
    }

    public class SQLiteMigrationHelper : ISQLiteMigrationHelper
    {
        private readonly SQLiteConnection _connection;

        private static readonly Regex SchemaRegex = new Regex(@"['\""\[](?<name>\w+)['\""\]]\s(?<schema>[\w-\s]+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly Regex IndexRegex = new Regex(@"\(""(?<col>.*)""\s(?<direction>ASC|DESC)\)$",
             RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public SQLiteMigrationHelper(IConnectionStringFactory connectionStringFactory, Logger logger)
        {
            try
            {
                _connection = new SQLiteConnection(connectionStringFactory.MainDbConnectionString);
                _connection.Open();
            }
            catch (Exception e)
            {
                logger.ErrorException("Couldn't open database " + connectionStringFactory.MainDbConnectionString, e);
                throw;
            }

        }

        private string GetOriginalSql(string tableName)
        {
            var command =
                new SQLiteCommand(string.Format("SELECT sql FROM sqlite_master WHERE type='table' AND name ='{0}'",
                                                tableName));

            command.Connection = _connection;

            return (string)command.ExecuteScalar();
        }

        public Dictionary<String, SQLiteColumn> GetColumns(string tableName)
        {
            var originalSql = GetOriginalSql(tableName);

            var matches = SchemaRegex.Matches(originalSql);

            return matches.Cast<Match>().ToDictionary(
                               match => match.Groups["name"].Value.Trim(),
                               match => new SQLiteColumn
                                   {
                                       Name = match.Groups["name"].Value.Trim(),
                                       Schema = match.Groups["schema"].Value.Trim()
                                   });
        }

        public List<SQLiteIndex> GetIndexes(string tableName)
        {
            var command = new SQLiteCommand(string.Format("SELECT sql FROM sqlite_master WHERE type='index' AND tbl_name ='{0}'", tableName));
            command.Connection = _connection;

            var reader = command.ExecuteReader();
            var sqls = new List<string>();

            while (reader.Read())
            {
                sqls.Add(reader[0].ToString());
            }


            var indexes = new List<SQLiteIndex>();

            foreach (var indexSql in sqls)
            {
                var newIndex = new SQLiteIndex();
                var matches = IndexRegex.Match(indexSql);

                newIndex.Column = matches.Groups["col"].Value;
                newIndex.Unique = indexSql.Contains("UNIQUE");
                newIndex.Table = tableName;

                indexes.Add(newIndex);
            }

            return indexes;
        }

        public void CreateTable(string tableName, IEnumerable<SQLiteColumn> values, IEnumerable<SQLiteIndex> indexes)
        {
            var columns = String.Join(",", values.Select(c => c.ToString()));

            ExecuteNonQuery("CREATE TABLE [{0}] ({1})", tableName, columns);

            foreach (var index in indexes)
            {
                ExecuteNonQuery("DROP INDEX {0}", index.IndexName);
                ExecuteNonQuery(index.CreateSql(tableName));
            }
        }

        public void CopyData(string sourceTable, string destinationTable, IEnumerable<SQLiteColumn> columns)
        {
            var originalCount = GetRowCount(sourceTable);

            var columnsToTransfer = String.Join(",", columns.Select(c => c.Name));

            var transferCommand = BuildCommand("INSERT INTO {0} SELECT {1} FROM {2};", destinationTable, columnsToTransfer, sourceTable);

            transferCommand.ExecuteNonQuery();

            var transferredRows = GetRowCount(destinationTable);


            if (transferredRows != originalCount)
            {
                throw new ApplicationException(string.Format("Expected {0} rows to be copied from [{1}] to [{2}]. But only copied {3}", originalCount, sourceTable, destinationTable, transferredRows));
            }
        }


        public void DropTable(string tableName)
        {
            var dropCommand = BuildCommand("DROP TABLE {0};", tableName);
            dropCommand.ExecuteNonQuery();
        }


        public void RenameTable(string tableName, string newName)
        {
            var renameCommand = BuildCommand("ALTER TABLE {0} RENAME TO {1};", tableName, newName);
            renameCommand.ExecuteNonQuery();
        }

        public int GetRowCount(string tableName)
        {
            var countCommand = BuildCommand("SELECT COUNT(*) FROM {0};", tableName);
            return Convert.ToInt32(countCommand.ExecuteScalar());
        }


        public SQLiteTransaction BeginTransaction()
        {
            return _connection.BeginTransaction();
        }

        private SQLiteCommand BuildCommand(string format, params string[] args)
        {
            var command = new SQLiteCommand(string.Format(format, args));
            command.Connection = _connection;
            return command;
        }



        private void ExecuteNonQuery(string command, params string[] args)
        {
            var sqLiteCommand = new SQLiteCommand(string.Format(command, args))
            {
                Connection = _connection
            };

            sqLiteCommand.ExecuteNonQuery();
        }


        public class SQLiteColumn
        {
            public string Name { get; set; }
            public string Schema { get; set; }

            public override string ToString()
            {
                return string.Format("[{0}] {1}", Name, Schema);
            }
        }

        public class SQLiteIndex
        {
            public string Column { get; set; }
            public string Table { get; set; }
            public bool Unique { get; set; }

            public override string ToString()
            {
                return string.Format("[{0}] Unique: {1}", Column, Unique);
            }

            public string IndexName
            {
                get
                {
                    return string.Format("IX_{0}_{1}", Table, Column);
                }
            }

            public string CreateSql(string tableName)
            {
                return string.Format(@"CREATE UNIQUE INDEX ""{2}"" ON ""{0}"" (""{1}"" ASC)", tableName, Column, IndexName);
            }
        }
    }


}