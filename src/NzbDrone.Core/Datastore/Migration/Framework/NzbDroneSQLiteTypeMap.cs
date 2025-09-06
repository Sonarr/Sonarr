using System.Data;
using FluentMigrator.Runner.Generators.Base;
using FluentMigrator.Runner.Generators.SQLite;

namespace NzbDrone.Core.Datastore.Migration.Framework;

// Based on https://github.com/fluentmigrator/fluentmigrator/blob/v6.2.0/src/FluentMigrator.Runner.SQLite/Generators/SQLite/SQLiteTypeMap.cs
public sealed class NzbDroneSQLiteTypeMap : TypeMapBase, ISQLiteTypeMap
{
    public bool UseStrictTables { get; }

    public NzbDroneSQLiteTypeMap(bool useStrictTables = false)
    {
        UseStrictTables = useStrictTables;

        SetupTypeMaps();
    }

    // Must be kept in sync with upstream
    protected override void SetupTypeMaps()
    {
        SetTypeMap(DbType.Binary, "BLOB");
        SetTypeMap(DbType.Byte, "INTEGER");
        SetTypeMap(DbType.Int16, "INTEGER");
        SetTypeMap(DbType.Int32, "INTEGER");
        SetTypeMap(DbType.Int64, "INTEGER");
        SetTypeMap(DbType.SByte, "INTEGER");
        SetTypeMap(DbType.UInt16, "INTEGER");
        SetTypeMap(DbType.UInt32, "INTEGER");
        SetTypeMap(DbType.UInt64, "INTEGER");

        if (!UseStrictTables)
        {
            SetTypeMap(DbType.Currency, "NUMERIC");
            SetTypeMap(DbType.Decimal, "NUMERIC");
            SetTypeMap(DbType.Double, "NUMERIC");
            SetTypeMap(DbType.Single, "NUMERIC");
            SetTypeMap(DbType.VarNumeric, "NUMERIC");
            SetTypeMap(DbType.Date, "DATETIME");
            SetTypeMap(DbType.DateTime, "DATETIME");
            SetTypeMap(DbType.DateTime2, "DATETIME");
            SetTypeMap(DbType.Time, "DATETIME");
            SetTypeMap(DbType.Guid, "UNIQUEIDENTIFIER");

            // Custom so that we can use DateTimeOffset in Postgres for appropriate DB typing
            SetTypeMap(DbType.DateTimeOffset, "DATETIME");
        }
        else
        {
            SetTypeMap(DbType.Currency, "TEXT");
            SetTypeMap(DbType.Decimal, "TEXT");
            SetTypeMap(DbType.Double, "REAL");
            SetTypeMap(DbType.Single, "REAL");
            SetTypeMap(DbType.VarNumeric, "TEXT");
            SetTypeMap(DbType.Date, "TEXT");
            SetTypeMap(DbType.DateTime, "TEXT");
            SetTypeMap(DbType.DateTime2, "TEXT");
            SetTypeMap(DbType.Time, "TEXT");
            SetTypeMap(DbType.Guid, "TEXT");

            // Custom so that we can use DateTimeOffset in Postgres for appropriate DB typing
            SetTypeMap(DbType.DateTimeOffset, "TEXT");
        }

        SetTypeMap(DbType.AnsiString, "TEXT");
        SetTypeMap(DbType.String, "TEXT");
        SetTypeMap(DbType.AnsiStringFixedLength, "TEXT");
        SetTypeMap(DbType.StringFixedLength, "TEXT");
        SetTypeMap(DbType.Boolean, "INTEGER");
    }

    public override string GetTypeMap(DbType type, int? size, int? precision)
    {
        return base.GetTypeMap(type, size: null, precision: null);
    }
}
