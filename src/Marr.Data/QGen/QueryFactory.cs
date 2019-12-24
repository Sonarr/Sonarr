using System;
using Marr.Data.Mapping;
using Marr.Data.QGen.Dialects;

namespace Marr.Data.QGen
{
    /// <summary>
    /// This class contains the factory logic that determines which type of IQuery object should be created.
    /// </summary>
    internal class QueryFactory
    {
        private const string DB_SQLiteClient = "System.Data.SQLite.SQLiteFactory";
        
        public static IQuery CreateUpdateQuery(ColumnMapCollection columns, IDataMapper dataMapper, string target, string whereClause)
        {
            Dialect dialect = CreateDialect(dataMapper);
            return new UpdateQuery(dialect, columns, dataMapper.Command, target, whereClause);
        }

        public static IQuery CreateInsertQuery(ColumnMapCollection columns, IDataMapper dataMapper, string target)
        {
            Dialect dialect = CreateDialect(dataMapper);
            return new InsertQuery(dialect, columns, dataMapper.Command, target);
        }

        public static IQuery CreateDeleteQuery(Dialect dialect, Table targetTable, string whereClause)
        {
            return new DeleteQuery(dialect, targetTable, whereClause);
        }

        public static IQuery CreateSelectQuery(TableCollection tables, IDataMapper dataMapper, string where, ISortQueryBuilder orderBy, bool useAltName)
        {
            Dialect dialect = CreateDialect(dataMapper);
            return new SelectQuery(dialect, tables, where, orderBy, useAltName);
        }

        public static IQuery CreateRowCountSelectQuery(TableCollection tables, IDataMapper dataMapper, string where, ISortQueryBuilder orderBy, bool useAltName)
        {
            SelectQuery innerQuery = (SelectQuery)CreateSelectQuery(tables, dataMapper, where, orderBy, useAltName);

            string providerString = dataMapper.ProviderFactory.ToString();
            switch (providerString)
            {
                case DB_SQLiteClient:
                    return new SqliteRowCountQueryDecorator(innerQuery);

                default:
                    throw new NotImplementedException("Row count has not yet been implemented for this provider.");
            }
        }

        public static IQuery CreatePagingSelectQuery(TableCollection tables, IDataMapper dataMapper, string where, ISortQueryBuilder orderBy, bool useAltName, int skip, int take)
        {
            SelectQuery innerQuery = (SelectQuery)CreateSelectQuery(tables, dataMapper, where, orderBy, useAltName);

            string providerString = dataMapper.ProviderFactory.ToString();
            switch (providerString)
            {
                case DB_SQLiteClient:
                    return new SqlitePagingQueryDecorator(innerQuery, skip, take);

                default:
                    throw new NotImplementedException("Paging has not yet been implemented for this provider.");
            }
        }

        public static Dialect CreateDialect(IDataMapper dataMapper)
        {
            string providerString = dataMapper.ProviderFactory.ToString();
            switch (providerString)
            {
                case DB_SQLiteClient:
                    return new SqliteDialect();

                default:
                    return new Dialect();
            }
        }
    }
}
