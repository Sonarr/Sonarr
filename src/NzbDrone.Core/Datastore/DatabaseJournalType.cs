using System.Data.SQLite;

namespace NzbDrone.Core.Datastore
{
    public enum DatabaseJournalType
    {
        Auto = SQLiteJournalModeEnum.Default,
        Wal = SQLiteJournalModeEnum.Wal,
        Truncate = SQLiteJournalModeEnum.Truncate,
        Delete = SQLiteJournalModeEnum.Delete
    }
}
