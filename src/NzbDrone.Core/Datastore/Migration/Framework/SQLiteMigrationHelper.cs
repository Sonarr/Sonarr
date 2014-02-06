using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public interface ISqLiteMigrationHelper
    {
        Dictionary<String, SQLiteColumn> GetColumns(string tableName);
        void CreateTable(string tableName, IEnumerable<SQLiteColumn> values, IEnumerable<SQLiteIndex> indexes);
        void CopyData(string sourceTable, string destinationTable, IEnumerable<SQLiteColumn> columns);
        void DropTable(string tableName);
        void RenameTable(string tableName, string newName);
        IEnumerable<IGrouping<T, KeyValuePair<int, T>>> GetDuplicates<T>(string tableName, string columnName);
        SQLiteTransaction BeginTransaction();
        List<SQLiteIndex> GetIndexes(string tableName);
        int ExecuteScalar(string command, params string[] args);
        void ExecuteNonQuery(string command, params string[] args);
    }

    public class SqLiteMigrationHelper : ISqLiteMigrationHelper
    {
        private readonly SQLiteConnection _connection;

        private static readonly Regex SchemaRegex = new Regex(@"['\""\[](?<name>\w+)['\""\]]\s(?<schema>[\w-\s]+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly Regex IndexRegex = new Regex(@"\((?:""|')(?<col>.*)(?:""|')\s(?<direction>ASC|DESC)\)$",
             RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public SqLiteMigrationHelper(IConnectionStringFactory connectionStringFactory, Logger logger)
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

            var sql = (string)command.ExecuteScalar();

            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new TableNotFoundException(tableName);
            }

            return sql;
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


        private static IEnumerable<T> ReadArray<T>(SQLiteDataReader reader)
        {
            while (reader.Read())
            {
                yield return (T)Convert.ChangeType(reader[0], typeof(T));
            }
        }

        public List<SQLiteIndex> GetIndexes(string tableName)
        {
            var command = new SQLiteCommand(string.Format("SELECT sql FROM sqlite_master WHERE type='index' AND tbl_name ='{0}'", tableName));
            command.Connection = _connection;

            var reader = command.ExecuteReader();
            var sqls = ReadArray<string>(reader).ToList();
            var indexes = new List<SQLiteIndex>();

            foreach (var indexSql in sqls)
            {
                var newIndex = new SQLiteIndex();
                var matches = IndexRegex.Match(indexSql);

                if (!matches.Success) continue;;

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
                ExecuteNonQuery("DROP INDEX IF EXISTS {0}", index.IndexName);
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

        public IEnumerable<IGrouping<T, KeyValuePair<int, T>>> GetDuplicates<T>(string tableName, string columnName)
        {
            var getDuplicates = BuildCommand("select id, {0} from {1}", columnName, tableName);

            var result = new List<KeyValuePair<int, T>>();

            using (var reader = getDuplicates.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(new KeyValuePair<int, T>(reader.GetInt32(0), (T)Convert.ChangeType(reader[1], typeof(T))));
                }
            }

            return result.GroupBy(c => c.Value).Where(g => g.Count() > 1);
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


        public void ExecuteNonQuery(string command, params string[] args)
        {
            var sqLiteCommand = new SQLiteCommand(string.Format(command, args))
            {
                Connection = _connection
            };

            sqLiteCommand.ExecuteNonQuery();
        }

        public int ExecuteScalar(string command, params string[] args)
        {
            var sqLiteCommand = new SQLiteCommand(string.Format(command, args))
            {
                Connection = _connection
            };

            return (int)sqLiteCommand.ExecuteScalar();
        }

        private class TableNotFoundException : NzbDroneException
        {
            public TableNotFoundException(string tableName)
                : base("Table [{0}] not found", tableName)
            {

            }
        }


    }
}