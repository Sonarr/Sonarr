using System;
using System.Linq;
using Marr.Data.Converters;
using Marr.Data.Mapping;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Datastore.Converters
{

    public class ProviderSettingConverter : EmbeddedDocumentConverter
    {
        public override object FromDB(ConverterContext context)
        {
            if (context.DbValue == DBNull.Value)
            {
                return DBNull.Value;
            }

            var stringValue = (string)context.DbValue;

            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }

            var ordinal = context.DataRecord.GetOrdinal("ConfigContract");

            var implementation = context.DataRecord.GetString(ordinal);


            var impType = typeof(IProviderConfig).Assembly.GetTypes().Single(c => c.Name == implementation);

            return Json.Deserialize(stringValue, impType);
        }

    }


    public class EmbeddedDocumentConverter : IConverter
    {
        public virtual object FromDB(ConverterContext context)
        {
            if (context.DbValue == DBNull.Value)
            {
                return DBNull.Value;
            }

            var stringValue = (string)context.DbValue;

            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }

            return Json.Deserialize(stringValue, context.ColumnMap.FieldType);
        }

        public object FromDB(ColumnMap map, object dbValue)
        {
            return FromDB(new ConverterContext { ColumnMap = map, DbValue = dbValue });
        }

        public object ToDB(object clrValue)
        {
            if (clrValue == null) return null;

            return clrValue.ToJson();
        }

        public Type DbType
        {
            get
            {
                return typeof(string);
            }
        }
    }
}