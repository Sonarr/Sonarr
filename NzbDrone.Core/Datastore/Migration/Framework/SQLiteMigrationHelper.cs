using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text.RegularExpressions;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public interface ISQLiteMigrationHelper
    {
        Dictionary<String, SQLiteMigrationHelper.SQLiteColumn> GetColumns(string tableName);
        void CreateTable(string tableName, IEnumerable<SQLiteMigrationHelper.SQLiteColumn> values);
        void CopyData(string sourceTable, string destinationTable, IEnumerable<SQLiteMigrationHelper.SQLiteColumn> columns);
        int GetRowCount(string tableName);
        void DropTable(string tableName);
        void RenameTable(string tableName, string newName);
        SQLiteTransaction BeginTransaction();
    }

    public class SQLiteMigrationHelper : ISQLiteMigrationHelper
    {
        private readonly SQLiteConnection _connection;

        private static readonly Regex SchemaRegex = new Regex(@"['\""\[](?<name>\w+)['\""\]]\s(?<schema>[\w-\s]+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public SQLiteMigrationHelper(IConnectionStringFactory connectionStringFactory)
        {
            _connection = new SQLiteConnection(connectionStringFactory.MainDbConnectionString);
            _connection.Open();
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

        public void CreateTable(string tableName, IEnumerable<SQLiteColumn> values)
        {
            var columns = String.Join(",", values.Select(c => c.ToString()));

            var command = new SQLiteCommand(string.Format("CREATE TABLE [{0}] ({1})", tableName, columns));
            command.Connection = _connection;

            command.ExecuteNonQuery();
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


        public class SQLiteColumn
        {
            public string Name { get; set; }
            public string Schema { get; set; }

            public override string ToString()
            {
                return string.Format("[{0}] {1}", Name, Schema);
            }
        }
    }


}