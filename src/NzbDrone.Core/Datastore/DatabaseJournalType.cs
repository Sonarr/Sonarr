using System.Data.SQLite;

namespace NzbDrone.Core.Datastore
{
    public enum DatabaseJournalType
    {
        Wal = SQLiteJournalModeEnum.Wal,
        Truncate = SQLiteJournalModeEnum.Truncate
    }
}
