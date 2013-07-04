using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text.RegularExpressions;
using FluentMigrator.Builders.Execute;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public class SQLiteAlter
    {
        private readonly SQLiteConnection _connection;

        private static readonly Regex SchemaRegex = new Regex(@"[\""\[](?<name>\w+)[\""\]]\s(?<schema>[\w-\s]+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public SQLiteAlter(string connectionString)
        {
            _connection = new SQLiteConnection(connectionString);
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

        public void CreateTable(string tableName, Dictionary<string, SQLiteColumn>.ValueCollection values)
        {
            var columns = String.Join(",", values.Select(c => c.ToString()));

            var command = new SQLiteCommand(string.Format("CREATE TABLE [{0}] ({1})", tableName, columns));
            command.Connection = _connection;

            command.ExecuteNonQuery();
        }
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