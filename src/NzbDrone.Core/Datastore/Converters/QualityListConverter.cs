using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;
using NzbDrone.Core.Qualities;
using System.Collections.Generic;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Datastore.Converters
{
    public class QualityListConverter : IConverter
    {
        public object FromDB(ConverterContext context)
        {
            if (context.DbValue == DBNull.Value)
            {
                return DBNull.Value;
            }

            var val = Convert.ToString(context.DbValue);

            var qualityList = Json.Deserialize<List<int>>(val).ConvertAll(Quality.FindById);

            return qualityList;
        }

        public object FromDB(ColumnMap map, object dbValue)
        {
            return FromDB(new ConverterContext { ColumnMap = map, DbValue = dbValue });
        }

        public object ToDB(object clrValue)
        {
            if (clrValue == DBNull.Value) return null;
            
            var qualityList = clrValue as List<Quality>;

            if (qualityList == null)
            {
                throw new InvalidOperationException("Can only store a list of qualities in this database column.");
            }

            var intList = qualityList.ConvertAll(v => v.Id);

            return intList.ToJson();
        }

        public Type DbType
        {
            get { return typeof(string); }
        }
    }
}
