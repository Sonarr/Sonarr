using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public interface ISQLiteAlter
    {
        void DropColumns(string tableName, IEnumerable<string> columns);
        void AddIndexes(string tableName, params SQLiteIndex[] indexes);
        void Nullify(string tableName, IEnumerable<string> columns);
    }

    public class SQLiteAlter : ISQLiteAlter
    {
        private readonly ISqLiteMigrationHelper _sqLiteMigrationHelper;

        public SQLiteAlter(ISqLiteMigrationHelper sqLiteMigrationHelper)
        {
            _sqLiteMigrationHelper = sqLiteMigrationHelper;
        }

        public void DropColumns(string tableName, IEnumerable<string> columns)
        {
            using (var transaction = _sqLiteMigrationHelper.BeginTransaction())
            {
                var originalColumns = _sqLiteMigrationHelper.GetColumns(tableName);
                var originalIndexes = _sqLiteMigrationHelper.GetIndexes(tableName);

                var newColumns = originalColumns.Where(c => !columns.Contains(c.Key)).Select(c => c.Value).ToList();
                var newIndexes = originalIndexes.Where(c => !columns.Contains(c.Column));

                CreateTable(tableName, newColumns, newIndexes);

                transaction.Commit();
            }
        }

        public void AddIndexes(string tableName, params SQLiteIndex[] indexes)
        {
            using (var transaction = _sqLiteMigrationHelper.BeginTransaction())
            {
                var columns = _sqLiteMigrationHelper.GetColumns(tableName).Select(c => c.Value).ToList();
                var originalIndexes = _sqLiteMigrationHelper.GetIndexes(tableName);

                var newIndexes = originalIndexes.Union(indexes);


                CreateTable(tableName, columns, newIndexes);

                transaction.Commit();
            }
        }

        public void Nullify(string tableName, IEnumerable<string> columns)
        {
            using (var transaction = _sqLiteMigrationHelper.BeginTransaction())
            {
                var originalColumns = _sqLiteMigrationHelper.GetColumns(tableName);
                var originalIndexes = _sqLiteMigrationHelper.GetIndexes(tableName);

                var newColumns = originalColumns.Select(c =>
                {
                    if (!columns.Contains(c.Key))
                    {
                        return c.Value;
                    }

                    if (!c.Value.Schema.Contains("NOT NULL") && c.Value.Schema.Contains("NULL"))
                    {
                        return c.Value;
                    }

                    if (c.Value.Schema.Contains("NOT NULL"))
                    {
                        c.Value.Schema = c.Value.Schema.Replace("NOT NULL", "NULL");
                        return c.Value;
                    }

                    c.Value.Schema += " NULL";

                    return c.Value;
                }).ToList();

                var newIndexes = originalIndexes;

                CreateTable(tableName, newColumns, newIndexes);

                transaction.Commit();
            }
        }

        private void CreateTable(string tableName, List<SQLiteColumn> newColumns, IEnumerable<SQLiteIndex> newIndexes)
        {
            var tempTableName = tableName + "_temp";

            _sqLiteMigrationHelper.CreateTable(tempTableName, newColumns, newIndexes);

            _sqLiteMigrationHelper.CopyData(tableName, tempTableName, newColumns);

            _sqLiteMigrationHelper.DropTable(tableName);

            _sqLiteMigrationHelper.RenameTable(tempTableName, tableName);
        }
    }
}