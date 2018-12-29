using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using Marr.Data.Converters;

namespace Marr.Data.Mapping
{
    internal class MappingHelper
    {
        private MapRepository _repos;
        private IDataMapper _db;

        public MappingHelper(IDataMapper db)
        {
            _repos = MapRepository.Instance;
            _db = db;
        }

        /// <summary>
        /// Instantiates an entity and loads its mapped fields with the data from the reader.
        /// </summary>
        public object CreateAndLoadEntity<T>(ColumnMapCollection mappings, DbDataReader reader, bool useAltName)
        {
            return CreateAndLoadEntity(typeof(T), mappings, reader, useAltName);
        }

        /// <summary>
        /// Instantiates an entity and loads its mapped fields with the data from the reader.
        /// </summary>
        /// <param name="entityType">The entity being created and loaded.</param>
        /// <param name="mappings">The field mappings for the passed in entity.</param>
        /// <param name="reader">The open data reader.</param>
        /// <param name="useAltNames">Determines if the column AltName should be used.</param>
        /// <returns>Returns an entity loaded with data.</returns>
        public object CreateAndLoadEntity(Type entityType, ColumnMapCollection mappings, DbDataReader reader, bool useAltName)
        {
            // Create new entity
            object ent = _repos.ReflectionStrategy.CreateInstance(entityType);
            return LoadExistingEntity(mappings, reader, ent, useAltName);
        }

        public object LoadExistingEntity(ColumnMapCollection mappings, DbDataReader reader, object ent, bool useAltName)
        {
            // Populate entity fields from data reader
            foreach (ColumnMap dataMap in mappings)
            {
                object dbValue = null;
                try
                {
                    string colName = dataMap.ColumnInfo.GetColumName(useAltName);
                    int ordinal = reader.GetOrdinal(colName);
                    dbValue = reader.GetValue(ordinal);

                    // Handle conversions
                    if (dataMap.Converter != null)
                    {
                        var convertContext = new ConverterContext
                        {
                            DbValue = dbValue,
                            ColumnMap = dataMap,
                            MapCollection = mappings,
                            DataRecord = reader
                        };

                        dbValue = dataMap.Converter.FromDB(convertContext);
                    }

                    if (dbValue != DBNull.Value && dbValue != null)
                    {
                        dataMap.Setter(ent, dbValue);
                    }
                }
                catch (Exception ex)
                {
                    string msg = string.Format("The DataMapper was unable to load the following field: '{0}' value: '{1}'. {2}",
                        dataMap.ColumnInfo.Name, dbValue, ex.Message);

                    throw new DataMappingException(msg, ex);
                }
            }

            PrepareLazyLoadedProperties(ent);

            return ent;
        }

        private void PrepareLazyLoadedProperties(object ent)
        {
            // Handle lazy loaded properties
            Type entType = ent.GetType();
            if (_repos.Relationships.ContainsKey(entType))
            {
                var provider = _db.ProviderFactory;
                var connectionString = _db.ConnectionString;
                Func<IDataMapper> dbCreate = () =>
                {
                    var db = new DataMapper(provider, connectionString);
                    db.SqlMode = SqlModes.Text;
                    return db;
                };

                var relationships = _repos.Relationships[entType];
                foreach (var rel in relationships.Where(r => r.IsLazyLoaded))
                {
                    var lazyLoaded = (ILazyLoaded)rel.LazyLoaded.Clone();
                    lazyLoaded.Prepare(dbCreate, ent);
                    rel.Setter(ent, lazyLoaded);
                }
            }
        }

        public T LoadSimpleValueFromFirstColumn<T>(DbDataReader reader)
        {
            try
            {
                return (T)reader.GetValue(0);
            }
            catch (Exception ex)
            {
                string firstColumnName = reader.GetName(0);
                string msg = string.Format("The DataMapper was unable to create a value of type '{0}' from the first column '{1}'.",
                    typeof(T).Name, firstColumnName);

                throw new DataMappingException(msg, ex);
            }
        }

        /// <summary>
        /// Creates all parameters for a SP based on the mappings of the entity,
        /// and assigns them values based on the field values of the entity.
        /// </summary>
        public void CreateParameters<T>(T entity, ColumnMapCollection columnMapCollection, bool isAutoQuery)
        {
            ColumnMapCollection mappings = columnMapCollection;

            if (!isAutoQuery)
            {
                // Order columns (applies to Oracle and OleDb only)
                mappings = columnMapCollection.OrderParameters(_db.Command);
            }

            foreach (ColumnMap columnMap in mappings)
            {
                if (columnMap.ColumnInfo.IsAutoIncrement)
                    continue;

                var param = _db.Command.CreateParameter();
                param.ParameterName = columnMap.ColumnInfo.Name;
                param.Size = columnMap.ColumnInfo.Size;
                param.Direction = columnMap.ColumnInfo.ParamDirection;

                object val = columnMap.Getter(entity);

                param.Value = val ?? DBNull.Value; // Convert nulls to DBNulls

                if (columnMap.Converter != null)
                {
                    param.Value = columnMap.Converter.ToDB(param.Value);
                }

                // Set the appropriate DbType property depending on the parameter type
                // Note: the columnMap.DBType property was set when the ColumnMap was created
                MapRepository.Instance.DbTypeBuilder.SetDbType(param, columnMap.DBType);

                _db.Command.Parameters.Add(param);
            }
        }

        /// <summary>
        /// Assigns the SP result columns to the passed in 'mappings' fields.
        /// </summary>
        public void SetOutputValues<T>(T entity, IEnumerable<ColumnMap> mappings)
        {
            foreach (ColumnMap dataMap in mappings)
            {
                object output = _db.Command.Parameters[dataMap.ColumnInfo.Name].Value;
                dataMap.Setter(entity, output);
            }
        }

        /// <summary>
        /// Assigns the passed in 'value' to the passed in 'mappings' fields.
        /// </summary>
        public void SetOutputValues<T>(T entity, IEnumerable<ColumnMap> mappings, object value)
        {
            foreach (ColumnMap dataMap in mappings)
            {
                dataMap.Setter(entity, Convert.ChangeType(value, dataMap.FieldType));
            }
        }

    }
}
