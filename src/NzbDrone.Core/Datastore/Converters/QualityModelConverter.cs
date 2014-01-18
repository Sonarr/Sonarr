using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;
using NzbDrone.Core.Qualities;
using System.Collections.Generic;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Datastore.Converters
{
    public class QualityModelConverter : IConverter
    {
        public object FromDB(ConverterContext context)
        {
            if (context.DbValue == DBNull.Value)
            {
                return new QualityModel();
            }

            var val = Convert.ToString(context.DbValue);

            var jsonObject = Json.Deserialize<Dictionary<string, object>>(val);

            return new QualityModel((Quality)Convert.ToInt32(jsonObject["id"]), Convert.ToBoolean(jsonObject["proper"]));
        }

        public object FromDB(ColumnMap map, object dbValue)
        {
            return FromDB(new ConverterContext { ColumnMap = map, DbValue = dbValue });
        }

        public object ToDB(object clrValue)
        {
            if (clrValue == DBNull.Value)
                clrValue = new QualityModel();

            var qualityModel = clrValue as QualityModel;

            if (qualityModel == null)
            {
                throw new InvalidOperationException("Can only store a QualityModel in this database column.");
            }

            var jsonObject = new Dictionary<string, object>();
            jsonObject["id"] = (int)qualityModel.Quality;
            jsonObject["proper"] = qualityModel.Proper;

            return jsonObject.ToJson();
        }

        public Type DbType
        {
            get { return typeof(string); }
        }
    }
}
