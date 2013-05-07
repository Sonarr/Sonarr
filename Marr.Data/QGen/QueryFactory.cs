using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using Marr.Data.QGen.Dialects;

namespace Marr.Data.QGen
{
    /// <summary>
    /// This class contains the factory logic that determines which type of IQuery object should be created.
    /// </summary>
    internal class QueryFactory
    {
        private const string DB_SqlClient = "System.Data.SqlClient.SqlClientFactory";
        private const string DB_OleDb = "System.Data.OleDb.OleDbFactory";
        private const string DB_SqlCeClient = "System.Data.SqlServerCe.SqlCeProviderFactory";
        private const string DB_SystemDataOracleClient = "System.Data.OracleClientFactory";
        private const string DB_OracleDataAccessClient = "Oracle.DataAccess.Client.OracleClientFactory";
        private const string DB_FireBirdClient = "FirebirdSql.Data.FirebirdClient.FirebirdClientFactory";
        private const string DB_SQLiteClient = "System.Data.SQLite.SQLiteFactory";
        
        public static IQuery CreateUpdateQuery(Mapping.ColumnMapCollection columns, IDataMapper dataMapper, string target, string whereClause)
        {
            Dialect dialect = CreateDialect(dataMapper);
            return new UpdateQuery(dialect, columns, dataMapper.Command, target, whereClause);
        }

        public static IQuery CreateInsertQuery(Mapping.ColumnMapCollection columns, IDataMapper dataMapper, string target)
        {
            Dialect dialect = CreateDialect(dataMapper);
            return new InsertQuery(dialect, columns, dataMapper.Command, target);
        }

        public static IQuery CreateDeleteQuery(Dialects.Dialect dialect, Table targetTable, string whereClause)
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
                case DB_SqlClient:
                    return new RowCountQueryDecorator(innerQuery);

                case DB_SqlCeClient:
                    return new RowCountQueryDecorator(innerQuery);

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
                case DB_SqlClient:
                    return new PagingQueryDecorator(innerQuery, skip, take);

                case DB_SqlCeClient:
                    return new PagingQueryDecorator(innerQuery, skip, take);

                case DB_SQLiteClient:
                    return new SqlitePagingQueryDecorator(innerQuery, skip, take);

                default:
                    throw new NotImplementedException("Paging has not yet been implemented for this provider.");
            }
        }

        public static Dialects.Dialect CreateDialect(IDataMapper dataMapper)
        {
            string providerString = dataMapper.ProviderFactory.ToString();
            switch (providerString)
            {
                case DB_SqlClient:
                    return new SqlServerDialect();

                case DB_OracleDataAccessClient:
                    return new OracleDialect();

                case DB_SystemDataOracleClient:
                    return new OracleDialect();

                case DB_SqlCeClient:
                    return new SqlServerCeDialect();

                case DB_FireBirdClient:
                    return new FirebirdDialect();

                case DB_SQLiteClient:
                    return new SqliteDialect();

                default:
                    return new Dialect();
            }
        }
    }
}
