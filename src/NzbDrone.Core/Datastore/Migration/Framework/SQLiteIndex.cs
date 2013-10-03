using System;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public class SQLiteIndex : IEquatable<SQLiteIndex>
    {
        public string Column { get; set; }
        public string Table { get; set; }
        public bool Unique { get; set; }

        public bool Equals(SQLiteIndex other)
        {
            return IndexName == other.IndexName;
        }

        public override int GetHashCode()
        {
            return IndexName.GetHashCode();
        }

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