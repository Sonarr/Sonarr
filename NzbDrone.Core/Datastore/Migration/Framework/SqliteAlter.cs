using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public interface ISQLiteAlter
    {
        void DropColumns(string tableName, IEnumerable<string> columns);
    }

    public class SQLiteAlter : ISQLiteAlter
    {
        private readonly ISQLiteMigrationHelper _sqLiteMigrationHelper;

        public SQLiteAlter(ISQLiteMigrationHelper sqLiteMigrationHelper)
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

                var tempTableName = tableName + "_temp";

                _sqLiteMigrationHelper.CreateTable(tempTableName, newColumns, newIndexes);

                _sqLiteMigrationHelper.CopyData(tableName, tempTableName, newColumns);

                _sqLiteMigrationHelper.DropTable(tableName);

                _sqLiteMigrationHelper.RenameTable(tempTableName, tableName);

                transaction.Commit();
            }
        }
    }
}